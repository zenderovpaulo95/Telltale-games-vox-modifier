using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telltale_games_vox_modifier
{
    public class Methods
    {
        public static int padSize(int size, int n)
        {
            int padLength = size % n;
            if (padLength != 0)
                padLength = n - padLength;

            return size + padLength;
        }

        public static string ConvertHexToString(byte[] array, int poz, int len_string, int ASCII_N)
        {
            try
            {
                byte[] temp_hex_string = new byte[len_string];
                Array.Copy(array, poz, temp_hex_string, 0, len_string);

                string result = Encoding.ASCII.GetString(temp_hex_string);
                return result;
            }
            catch
            { return "error"; }
        }
        private static int SearchBinText(byte[] block, string text)
        {
            for (int i = 0; i < block.Length; i++)
            {
                if (i + text.Length < block.Length)
                {
                    byte[] tmp = new byte[text.Length];
                    Array.Copy(block, i, tmp, 0, tmp.Length);
                    if (Encoding.ASCII.GetString(tmp) == text)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static bool ContainsString(byte[] bytes, string text)
        {
            return SearchBinText(bytes, text) != -1;
        }

        public static byte[] stringToKey(string key) //Конвертация строки с hex-значениями в байты
        {
            byte[] result = null;

            if ((key.Length % 2) == 0) //Проверка на чётность строки
            {
                for (int i = 0; i < key.Length; i++) //Проверка на наличие пробелов
                {
                    if (key[i] == ' ')
                    {
                        return null;
                    }
                }

                result = new byte[key.Length / 2];

                for (int i = 0; i < key.Length; i += 2) //Попытки преобразовать строку в массив байт
                {
                    bool remake = byte.TryParse(key.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null as IFormatProvider, out result[i / 2]);

                    if (remake == false) //Если что-то пошло не так, то очистим массив байт и вернём null
                    {
                        return null;
                    }
                }

            }

            return result;
        }
    }
}
