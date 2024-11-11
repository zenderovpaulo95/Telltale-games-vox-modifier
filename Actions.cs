using NAudio.Wave;
using SpeexSharp.Native;
using SpeexSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Telltale_games_vox_modifier
{
    public class Actions
    {
        public static string Repack(FileInfo inputFI, string outputFile, byte[] key, bool needEncrypt)
        {
            try
            {
                FileStream fs = new FileStream(inputFI.FullName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] header = br.ReadBytes(4);
                //bool needEncrypt = Encoding.ASCII.GetString(header) != "ERTM";
                int count = br.ReadInt32();

                int[] lens = new int[count];
                ulong[] crcs = new ulong[count];
                string[] names = new string[count];
                uint[] vals = new uint[count];

                bool isCRC = true;

                int curr = (int)br.BaseStream.Position;
                byte[] checkHeader = br.ReadBytes(16);
                br.BaseStream.Seek(curr, SeekOrigin.Begin);

                bool hasByteVal = true;

                if (Methods.ContainsString(checkHeader, "class") || Methods.ContainsString(checkHeader, "struct"))
                {
                    for (int i = 0; i < count; i++)
                    {
                        lens[i] = br.ReadInt32();
                        byte[] tmp = br.ReadBytes(lens[i]);
                        names[i] = Encoding.ASCII.GetString(tmp);
                        vals[i] = br.ReadUInt32();
                        isCRC = false;
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        crcs[i] = br.ReadUInt64();
                        vals[i] = br.ReadUInt32();
                    }
                }

                int pos = (int)br.BaseStream.Position;
                byte zero = br.ReadByte();

                if (zero != 0x30 && zero != 0x31)
                {
                    hasByteVal = false;
                    br.BaseStream.Seek(-1, SeekOrigin.Current);
                }

            reread:
                float time = br.ReadSingle();
                int dataSize = br.ReadInt32();
                int frameSize = br.ReadInt32();
                int freq = br.ReadInt32();
                int noChnls = br.ReadInt32();
                int blSize = br.ReadInt32();
                int blCount = br.ReadInt32();

                //Check if block size is correct and try read again
                if (fs.Length - (int)fs.Position - (4 * blCount) != dataSize)
                {
                    br.BaseStream.Seek(pos, SeekOrigin.Begin);
                    hasByteVal = false;
                    goto reread;
                }

                br.Close();
                fs.Close();

                string fileName = inputFI.FullName.Remove(inputFI.FullName.Length - 3, 3) + "wav";
                var afr = new AudioFileReader(fileName);
                noChnls = afr.WaveFormat.Channels;
                int bitsPerSample = afr.WaveFormat.BitsPerSample;
                int sampleRate = afr.WaveFormat.SampleRate;
                time = (float)afr.TotalTime.TotalSeconds;

                byte[] data = new byte[afr.Length];

                int bytesRead = -1;


                if (noChnls == 2)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var newFormat = new NAudio.Wave.WaveFormat(sampleRate / 2, bitsPerSample, noChnls);

                        using (var tmpWav = new MediaFoundationResampler(afr, newFormat))
                        {
                            WaveFileWriter.WriteWavFileToStream(ms, tmpWav);
                        }

                        ms.Seek(0, SeekOrigin.Begin);

                        using (var wavStr = new WaveFileReader(ms))
                        {
                            data = new byte[wavStr.Length];

                            bytesRead = wavStr.ToSampleProvider().ToWaveProvider16().Read(data, 0, data.Length);
                        }
                    }
                }
                else
                {
                    bytesRead = afr.ToWaveProvider16().Read(data, 0, data.Length);
                }

                afr.Close();

                unsafe
                {
                    SpeexSharp.Native.SpeexBits bits;
                    Speex.BitsInit(&bits);

                    //if (noChnls == 2) sampleRate *= 2;

                    SpeexSharp.Native.SpeexMode* nativeMode = SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Narrowband);

                    switch (frameSize)
                    {
                        case 320:
                            //nativeMode = noChnls == 2 ? SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Narrowband) : SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Wideband);
                            nativeMode = SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Wideband);
                            break;

                        case 640:
                            //nativeMode = noChnls == 2 ? SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Wideband) : SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.UltraWideband);
                            nativeMode = SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.UltraWideband);
                            break;
                    }

                    void* encState = Speex.EncoderInit(nativeMode);

                    int encFrame, Enhance;
                    Speex.EncoderCtl(encState, (int)GetCoderParameter.FrameSize, &encFrame);
                    Enhance = 1;
                    Speex.EncoderCtl(encState, (int)SetCoderParameter.Enh, &Enhance);
                    Speex.EncoderCtl(encState, (int)SetCoderParameter.SamplingRate, &sampleRate);
                    int quality = 5;
                    //Speex.EncoderCtl(encState, (int)GetCoderParameter.SamplingRate, &smpRate);
                    Speex.EncoderCtl(encState, (int)SetCoderParameter.Quality, &quality);

                    int paddedLength = Methods.padSize(bytesRead / 2, encFrame);
                    //int paddedLength = padSize(bytesRead / 2, frameSize);
                    short[] samples = new short[paddedLength];
                    int offt = 0;

                    for (int i = 0; i < paddedLength; i++)
                    {
                        samples[i] = BitConverter.ToInt16(data, offt);
                        offt += 2;
                    }

                    MemoryStream speexStream = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(speexStream);

                    blCount = samples.Length / encFrame;
                    int[] offset = new int[blCount + 1];
                    int off = 0;
                    int encOff = 0;
                    byte[] tmp;

                    using (MemoryStream msw = new MemoryStream())
                    {
                        offset[0] = off;
                        //First block "Encoded with Speex speex-1.0.4"
                        tmp = new byte[] { 0x1E, 0x00, 0x00, 0x00, 0x45, 0x6E, 0x63, 0x6F, 0x64, 0x65, 0x64, 0x20, 0x77, 0x69, 0x74, 0x68, 0x20, 0x53, 0x70, 0x65, 0x65, 0x78, 0x20, 0x73, 0x70, 0x65, 0x65, 0x78, 0x2D, 0x31, 0x2E, 0x30, 0x2E, 0x34, 0x00, 0x00, 0x00, 0x00 };
                        msw.Write(tmp);
                        off += tmp.Length;

                        tmp = msw.ToArray();

                        if (needEncrypt)
                        {
                            BlowFishCS.BlowFish encbl = new BlowFishCS.BlowFish(key, 2);
                            tmp = encbl.Crypt_ECB(tmp, 2, false);
                        }

                        bw.Write(tmp);
                    }

                    // encode
                    for (int i = 1; i < blCount; i++)
                    {
                        offset[i] = off;

                        Speex.BitsReset(&bits);
                        Span<short> inputSpan = new Span<short>(samples, encOff, encFrame);

                        byte[] bytes = new byte[encFrame];

                        fixed (short* samplePtr = inputSpan)
                        {
                            Speex.EncodeInt(encState, samplePtr, &bits);

                            if (noChnls == 2)
                            {
                                Speex.EncodeStereoInt(samplePtr, encFrame, &bits);
                            }

                            fixed (byte* b = bytes)
                            {
                                int tmpCount = Speex.BitsWrite(&bits, b, encFrame);
                                off += tmpCount;
                                tmp = new byte[tmpCount];
                            }

                            Array.Copy(bytes, 0, tmp, 0, tmp.Length);

                            if ((i % 64 == 0) && needEncrypt)
                            {
                                BlowFishCS.BlowFish encbl = new BlowFishCS.BlowFish(key, 2);
                                tmp = encbl.Crypt_ECB(tmp, 2, false);
                            }

                            bw.Write(tmp);
                            encOff += encFrame;
                        }

                    end:
                        int skip = 1;
                    }

                    byte[] t = speexStream.ToArray();
                    dataSize = t.Length;
                    blSize = 8 + (4 * blCount);

                    bw.Close();
                    speexStream.Close();

                    if (File.Exists(outputFile + Path.DirectorySeparatorChar + inputFI.Name)) File.Delete(outputFile + Path.DirectorySeparatorChar + inputFI.Name);
                    fs = new FileStream(outputFile + Path.DirectorySeparatorChar + inputFI.Name, FileMode.CreateNew);

                    fs.Write(header, 0, header.Length);
                    tmp = BitConverter.GetBytes(count);
                    fs.Write(tmp, 0, tmp.Length);

                    if (isCRC)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            tmp = BitConverter.GetBytes(crcs[i]);
                            fs.Write(tmp, 0, tmp.Length);
                            tmp = BitConverter.GetBytes(vals[i]);
                            fs.Write(tmp, 0, tmp.Length);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            tmp = BitConverter.GetBytes(lens[i]);
                            fs.Write(tmp, 0, tmp.Length);
                            tmp = Encoding.ASCII.GetBytes(names[i]);
                            fs.Write(tmp, 0, tmp.Length);
                            tmp = BitConverter.GetBytes(vals[i]);
                            fs.Write(tmp, 0, tmp.Length);
                        }
                    }

                    if (hasByteVal)
                    {
                        tmp = new byte[1];
                        tmp[0] = zero;
                        fs.Write(tmp, 0, tmp.Length);
                    }

                    tmp = BitConverter.GetBytes(time);
                    fs.Write(tmp, 0, tmp.Length);
                    tmp = BitConverter.GetBytes(dataSize);
                    fs.Write(tmp, 0, tmp.Length);
                    //if (noChnls == 2) encFrame *= 2;
                    //tmp = BitConverter.GetBytes(encFrame);
                    tmp = BitConverter.GetBytes(frameSize);
                    fs.Write(tmp, 0, tmp.Length);
                    //if (noChnls == 2) sampleRate *= 2;
                    tmp = BitConverter.GetBytes(sampleRate);
                    //if (noChnls == 2) smpRate *= 2;
                    //tmp = BitConverter.GetBytes(smpRate);
                    fs.Write(tmp, 0, tmp.Length);
                    tmp = BitConverter.GetBytes(noChnls);
                    fs.Write(tmp, 0, tmp.Length);
                    tmp = BitConverter.GetBytes(blSize);
                    fs.Write(tmp, 0, tmp.Length);
                    tmp = BitConverter.GetBytes(blCount);
                    fs.Write(tmp, 0, tmp.Length);

                    for (int i = 0; i < blCount; i++)
                    {
                        byte[] c = BitConverter.GetBytes(offset[i]);
                        fs.Write(c, 0, c.Length);
                    }

                    fs.Write(t, 0, t.Length);
                    fs.Close();

                    Speex.EncoderDestroy(encState);

                    return "File " + inputFI.Name + " successfully repacked";
                }
            }
            catch (Exception ex)
            {
                return "Error with file " + inputFI.Name + ": " + ex.Message;
            }
        }

        public static string Unpack(FileInfo fi, string outputFolder, byte[] key, bool needDecrypt)
        {
            try
            {
                FileStream fs = new FileStream(fi.FullName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] header = br.ReadBytes(4);
                int count = br.ReadInt32();

                byte[] check = br.ReadBytes(16);
                br.BaseStream.Seek(8, SeekOrigin.Begin);

                int retPos = 8;

                if (Methods.ContainsString(check, "class") || Methods.ContainsString(check, "struct"))
                {
                    for (int i = 0; i < count; i++)
                    {
                        int len = br.ReadInt32();
                        byte[] tmp = br.ReadBytes(len);
                        tmp = br.ReadBytes(4);
                        retPos += 4 + len + 4;
                    }
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        byte[] tmp = br.ReadBytes(8);
                        tmp = br.ReadBytes(4);
                        retPos += 12;
                    }
                }

                byte zero = br.ReadByte();

                if ((zero != 0x30) && (zero != 0x31))
                {
                    br.BaseStream.Seek(-1, SeekOrigin.Current);
                }

            reread:
                float time = br.ReadSingle();
                int dataSize = br.ReadInt32();
                int frameSize = br.ReadInt32();
                int freq = br.ReadInt32();
                int noChnls = br.ReadInt32();
                int blSize = br.ReadInt32();
                int blCount = br.ReadInt32();

                //Check if block size is correct and try read again
                if (fs.Length - (int)fs.Position - (4 * blCount) != dataSize)
                {
                    br.BaseStream.Seek(retPos, SeekOrigin.Begin);
                    goto reread;
                }

                int[] blocks = new int[blCount];

                MemoryStream ms = new MemoryStream();
                BinaryWriter bwms = new BinaryWriter(ms);

                Speex.CompatibilityMode = true;

                unsafe
                {
                    SpeexSharp.Native.SpeexBits bits;
                    Speex.BitsInit(&bits);
                    //if (noChnls == 2) freq /= 2;


                    SpeexSharp.Native.SpeexMode* nativeMode = SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Narrowband);

                    switch (frameSize)
                    {
                        case 320:
                            //nativeMode = noChnls == 2 ? SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Narrowband) : SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Wideband);
                            nativeMode = SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Wideband);
                            break;

                        case 640:
                            //nativeMode = noChnls == 2 ? SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.Wideband) : SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.UltraWideband);
                            nativeMode = SpeexUtils.GetNativeMode(SpeexSharp.SpeexMode.UltraWideband);
                            break;
                    }

                    void* decState = Speex.DecoderInit(nativeMode);
                    int decFrame, Enhance;
                    Speex.DecoderCtl(decState, (int)GetCoderParameter.FrameSize, &decFrame);
                    Enhance = 1;
                    Speex.DecoderCtl(decState, (int)SetCoderParameter.Enh, &Enhance);

                    SpeexStereoState* sss = Speex.StereoStateInit();

                    for (int i = 0; i < blCount; i++)
                    {
                        blocks[i] = br.ReadInt32();
                    }

                    List<byte[]> bls = new List<byte[]>();

                    for (int i = 0; i < blCount; i++)
                    {
                        byte[] bl = i + 1 == blCount ? br.ReadBytes((int)br.BaseStream.Length - blocks[i]) : br.ReadBytes(blocks[i + 1] - blocks[i]);
                        bls.Add(bl);
                    }

                    //FileStream tfs = new FileStream("test.bin", FileMode.CreateNew);

                    for (int i = 0; i < bls.Count(); i++)
                    {
                        //tfs.Write(bls[i], 0, bls[i].Length);

                        if (i % 64 == 0 && needDecrypt)
                        {
                            BlowFishCS.BlowFish decbl = new BlowFishCS.BlowFish(key, 2);
                            bls[i] = decbl.Crypt_ECB(bls[i], 2, true);
                        }

                        if ((i == 0) && Methods.ContainsString(bls[i], "Encoded with Speex"))
                        {
                            goto end;
                        }

                        int blSz = (int)bls[i].Length;

                        fixed (byte* b = bls[i])
                        {
                            short[] vals = noChnls == 2 ? new short[decFrame * 2] : new short[decFrame];

                            Speex.BitsReset(&bits);
                            //Speex.BitsReadFrom(&bits, b, vals.Length);
                            Speex.BitsReadFrom(&bits, b, blSz);
                            int k = -1;

                            //do
                            //{
                            fixed (short* v = vals)
                            {
                                k = Speex.DecodeInt(decState, &bits, &v[0]);

                                //if (k == 0)
                                //{
                                if (noChnls == 2) Speex.DecodeStereoInt(&v[0], decFrame, sss);

                                Span<byte> res = MemoryMarshal.Cast<short, byte>(vals);

                                bwms.Write(res);
                                //}
                            }
                            //} while (k != 0);
                        }

                    end:
                        int skip = 1;
                    }

                    //tfs.Close();

                    Speex.DecoderDestroy(decState);
                    Speex.StereoStateDestroy(sss);
                }

                byte[] result = ms.ToArray();

                NAudio.Wave.WaveFormat wavformat = new NAudio.Wave.WaveFormat(freq, 16, noChnls);
                WaveFileWriter wfw = new WaveFileWriter(outputFolder + Path.DirectorySeparatorChar + fi.Name.Remove(fi.Name.Length - 3, 3) + "wav", wavformat);
                wfw.Write(result, 0, result.Length);
                wfw.Close();

                bwms.Close();
                ms.Close();
                br.Close();
                fs.Close();

                return "File " + fi.Name + " successfully extracted";
            }
            catch (Exception ex)
            {
                return "Error in " + fi.Name + ": " + ex.Message;
            }
        }
    }
}
