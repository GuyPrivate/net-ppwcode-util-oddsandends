﻿// Copyright 2014 by PeopleWare n.v..
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PPWCode.Util.OddsAndEnds.II.Identification
{
    public static class Validation
    {
        public const int LengthKbo = 10;
        public const int LengthVat = 9;
        public const int LengthRrn = 11;
        public const int LengthRsz = 10;

        [Pure]
        public static long? ValidDmfaNumber(string dmfaLotNbr)
        {
            long? result;

            if (new Regex(@"DMFA(A|P)\d{9}(\d|[A-Z])").IsMatch(dmfaLotNbr))
            {
                result = long.Parse(dmfaLotNbr.Substring(5, 9));
            }
            else if (new Regex(@"\d{10}").IsMatch(dmfaLotNbr) || new Regex(@"\d{9}").IsMatch(dmfaLotNbr))
            {
                result = long.Parse(dmfaLotNbr.Substring(0, 9));
            }
            else
            {
                result = null;
            }

            return result;
        }

        [Pure]
        public static bool ValidRrn(string rrn)
        {
            string digitStream = PadRrn(GetDigitStream(rrn));
            return StrictValidRrn(digitStream);
        }

        [Pure]
        public static bool StrictValidRrn(string rrn)
        {
            if (rrn.Length == LengthRrn)
            {
                string number = rrn.Substring(0, 9);
                long numberBefore2000 = long.Parse(number);
                long numberAfter2000 = long.Parse(string.Concat('2', number));
                int rest = 97 - int.Parse(rrn.Substring(9, 2));
                return numberBefore2000 % 97 == rest || numberAfter2000 % 97 == rest;
            }

            return false;
        }

        [Pure]
        public static string PadRrn(string rrn)
        {
            return string.IsNullOrEmpty(rrn) ? rrn : rrn.PadLeft(LengthRrn, '0');
        }

        private static void ParseRrn(string rrn, out DateTime? birthdate, out int sexe)
        {
            birthdate = null;
            sexe = 9;
            string digitStream = GetDigitStream(rrn);
            if (ValidRrn(rrn) && (digitStream.Length > 0))
            {
                bool calcSexe = false;
                bool calcBirthDate = false;
                int yy = int.Parse(digitStream.Substring(0, 2));
                int mm = int.Parse(digitStream.Substring(2, 2));
                int dd = int.Parse(digitStream.Substring(4, 2));
                int vvv = int.Parse(digitStream.Substring(6, 3));

                int yyOffset;
                {
                    long numberBefore2000 = long.Parse(digitStream.Substring(0, 9));
                    int rest = 97 - int.Parse(digitStream.Substring(9, 2));
                    yyOffset = numberBefore2000 % 97 == rest ? 1900 : 2000;
                }

                yy = yy + yyOffset;

                if (mm < 20)
                {
                    calcSexe = true;
                    calcBirthDate = true;
                }
                else if ((20 <= mm) && (mm < 40))
                {
                    mm -= 20;
                    calcBirthDate = true;
                }
                else if ((40 <= mm) && (mm < 60))
                {
                    mm -= 40;
                    calcSexe = true;
                    calcBirthDate = true;
                }

                if (calcSexe)
                {
                    sexe = (vvv == 0 || vvv == 999) ? 0 : ((vvv % 2) == 1) ? 1 : 2;
                }

                if (calcBirthDate)
                {
                    try
                    {
                        birthdate = new DateTime(yy, mm, dd);
                        if (birthdate > DateTime.Today)
                        {
                            birthdate = null;
                        }
                    }
                    catch
                    {
                        birthdate = null;
                    }
                }
            }
        }

        [Pure]
        public static DateTime? GetBirthDateFromRrn(string rrn)
        {
            DateTime? birthDate;
            int sexe;
            ParseRrn(rrn, out birthDate, out sexe);
            return birthDate;
        }

        [Pure]
        public static string GetDigitStream(string stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder(stream.Length);
            foreach (char ch in stream.Where(char.IsDigit))
            {
                sb.Append(ch);
            }

            return sb.ToString();
        }

        [Pure]
        public static bool ValidRsz(string rsz)
        {
            string digitStream = PadRsz(GetDigitStream(rsz));
            return StrictValidRsz(digitStream);
        }

        [Pure]
        public static bool StrictValidRsz(string rsz)
        {
            bool result = false;
            if (rsz.Length == LengthRsz)
            {
                long rest = 96 - ((long.Parse(rsz.Substring(0, 8)) * 100) % 97);

                if (rest == 0)
                {
                    rest = 97;
                }

                result = rest == long.Parse(rsz.Substring(8, 2));
            }

            return result;
        }

        [Pure]
        public static string PadRsz(string rsz)
        {
            return string.IsNullOrEmpty(rsz) ? rsz : rsz.PadLeft(LengthRsz, '0');
        }

        [Pure]
        public static bool ValidKbo(string kbo)
        {
            string digitStream = PadKbo(GetDigitStream(kbo));
            return StrictValidKbo(digitStream);
        }

        [Pure]
        public static bool StrictValidKbo(string kbo)
        {
            bool result = false;
            if (kbo.Length == LengthKbo)
            {
                long rest = 97 - (long.Parse(kbo.Substring(0, 8)) % 97);
                result = rest == long.Parse(kbo.Substring(8, 2));
            }

            return result;
        }

        [Pure]
        public static string PadKbo(string kbo)
        {
            return string.IsNullOrEmpty(kbo) ? kbo : kbo.PadLeft(LengthKbo, '0');
        }

        [Pure]
        public static bool ValidVat(string vat)
        {
            string digitStream = GetDigitStream(vat);
            if (digitStream.Length < LengthVat)
            {
                digitStream = digitStream.PadLeft(LengthVat, '0');
            }

            return StrictValidVat(digitStream);
        }

        [Pure]
        public static bool StrictValidVat(string vat)
        {
            bool result = false;
            if (vat.Length == LengthVat)
            {
                long rest = 97 - (long.Parse(vat.Substring(0, 7)) % 97);
                result = rest == long.Parse(vat.Substring(7, 2));
            }

            return result;
        }
    }
}