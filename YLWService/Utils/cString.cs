using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSoft.HIS
{
    public class cString
    {
        public static string Trim(string src)
        {
            return src.Trim();
        }

        public static string ReplStr(string expression, string Delim, int Piece, string Str)
        {
            int i, Cnt, DelLen, Pos;
            string CurCh;
            i = 1;
            Cnt = 0;
            do {
                CurCh = cString.Mid(expression, i, 1);
                //  expressin내에 원하는 위치(Piece)가 없는 경우
                if (CurCh == "") {
                    return expression + Delim + Str;
                }
                if (CurCh == Delim) {
                    Cnt = Cnt + 1;
                    if (Piece == 1) {
                        break;
                    }
                }
                i = i + 1;
            } while (Cnt <= Piece - 1);
            Pos = i - 1;
            // first character is the delimit or piece=1
            if (Piece == 1) {
                DelLen = 0;
                while(CurCh != Delim) {
                    CurCh = cUtil.Mid(expression, i, 1);
                    i = i + 1;
                    DelLen = DelLen + 1;
                }
                return Str + cUtil.Mid(expression, DelLen + 1, int.MaxValue);
            }
            //  expression내에 delimit가 없는 경우
            if (Cnt == 0) {
                return expression + Delim + Str;
            }
            // Replace를 위해 삭제될 String Length
            DelLen = 0;
            CurCh = "";
            while(CurCh != Delim) {
                CurCh = cUtil.Mid(expression, i, 1);
                //The Last character is delimit
                if (CurCh == "") {
                    DelLen = DelLen + 1;
                    // ReplStr = expression + str
                    break;
                }
                i = i + 1;
                DelLen = DelLen + 1;
            }
            return Mid(expression, 1, Pos) + Str + Mid(expression, Pos + DelLen);
        }

        public static string PatTr(string Src, string Pat = "[+")
        {
            // TODO: Add your implementation code here
            string rtn = "", SrcT, Tmp;
            SrcT = Src;
            if (Pat == ("[+")){
                Tmp = ExtP(ref SrcT, "[");
                while (SrcT != ("")){
                    Tmp = ExtP(ref SrcT, "]");
                    if (Tmp != ("")){
                        if (rtn != ("")) rtn += " ";
                        rtn += Tmp;
                    };
                    Tmp = ExtP(ref SrcT, "[");
                };
            }
        	else if (Pat == ("+[")){
                Tmp = ExtP(ref SrcT, "[");
                while (Tmp != ("") || SrcT != ("")){
                    if (Tmp != ("")){
                        if (rtn != ("")) rtn += " ";
                        rtn += Tmp;
                    };
                    Tmp = ExtP(ref SrcT, "]");
                    Tmp = ExtP(ref SrcT, "[");
                };
            }
	        else rtn = Src;
            return rtn;
        }

        public static long BtoL(string Src)
        {
            return cConvert.ToLong(Src);
        }

        public static double BtoD(string Src)
        {
            return cConvert.ToDouble(Src);
        }

        public static string LtoB(long Src)
        {
            return cConvert.ToString(Src);
        }

        public static string DtoB(double Src)
        {
            return cConvert.ToString(Src);
        }

        public static string Tr(string Src, string Trans, string Desti)
        {
            return cUtil.Tr(Src, Trans, Desti);
        }

        public static string GetP(string Src, string Deli, long Pos)
        {
            return GetP(Src, Deli, Convert.ToInt32(Pos));
        }

        public static string GetP(string Src, string Deli, int Pos)
        {
            return cUtil.GetP(Src, Deli, Pos);
        }

        public static string GetP(string Src, int Depth, long Pos)
        {
            return GetP(Src, Depth, Convert.ToInt32(Pos));
        }

        public static string GetP(string Src, int Depth, int Pos)
        {
            return cUtil.GetP(Src, Depth, Pos);
        }

        public static string ExtP(ref string Src, string Deli)
        {
            return cUtil.Shift(ref Src, Deli);
        }

        public static string ExtP(ref string Src, int Depth)
        {
            return cUtil.Shift(ref Src, Depth);
        }

        public static string GetPs(string Src, string Deli, int FrP, int ToP)
        {
            return cUtil.GetPs(Src, Deli, FrP, ToP);
        }

        public static long GetPlen(string Src, string Deli)
        {
            return cUtil.GetPlen(Src, Deli);
        }

        public static string SetP(string Src, string Deli, int Pos, string Trans)
        {
            return cUtil.SetP(Src, Deli, Pos, Trans);
        }

        public static string Mid(string Src, long FrP, long ToP = -1)
        {
            return Mid(Src, Convert.ToInt32(FrP), Convert.ToInt32(ToP));
        }

        public static string Mid(string Src, int FrP, int ToP = -1)
        {
            return cUtil.Mid(Src, FrP, (ToP == -1 ? FrP : ToP));
        }

        public static string RMid(string Src, long FrP, long ToP = -1)
        {
            return RMid(Src, Convert.ToInt32(FrP), Convert.ToInt32(ToP));
        }

        public static string RMid(string Src, int FrP, int ToP = -1)
        {
            return cUtil.RMid(Src, FrP, (ToP == -1 ? FrP : ToP));
        }

        public static string HMid(string Src, long FrP, long ToP = -1)
        {
            return HMid(Src, Convert.ToInt32(FrP), Convert.ToInt32(ToP));
        }

        public static string HMid(string Src, int FrP, int ToP = -1)
        {
            return cUtil.HMid(Src, FrP, (ToP == -1 ? FrP : ToP));
        }

        public static string HRMid(string Src, long FrP, long ToP = -1)
        {
            return HRMid(Src, Convert.ToInt32(FrP), Convert.ToInt32(ToP));
        }

        public static string HRMid(string Src, int FrP, int ToP = -1)
        {
            return cUtil.HRMid(Src, FrP, (ToP == -1 ? FrP : ToP));
        }

        public static string FN_MONEY(string sSrc, string Default = "")
        {
            double Src = cConvert.ToDouble(sSrc);
            if (Src == 0) return Default;
            return FN(Src, 0);
        }

        public static string FN_MONEY(double Src, string Default = "")
        {
            if (Src == 0) return Default;
            return FN(Src, 0);
        }

        public static string FN(string sSrc, int slen = 0)
        {
            double Src = cConvert.ToDouble(sSrc);
            if (Src == 0) return "0";
            return FN(Src, slen);
        }

        public static string FN(double Src, int slen = 0)
        {
            string fg, rtn, tmp, tmp1, tmp2;
            int c, Len, fLen, Exp;
            fg = "";
            if ((Src < 0) && (Src != 0)) { fg = "-"; Src = (-1) * Src; };
            tmp = cConvert.ToString(Src);
            Exp = cConvert.ToInt(GetP(tmp, "E", 2));
            if (Exp < 0)
            {
                Exp = -1 * Exp;
                tmp = SetP("", "0", Exp + 1, "") + Tr(tmp, ".", "");
                tmp = Mid(tmp, 1) + "." + Mid(tmp, 2, tmp.Length);
            };
            tmp1 = GetP(tmp, ".", 1); tmp2 = GetP(tmp, ".", 2);
            if (slen > 0)
            {
                tmp2 += SetP("", "0", slen + 1, "");
                tmp2 = "." + Mid(tmp2, 1, slen);
            }
            else tmp2 = "";
            rtn = "";
            Len = tmp1.Length; fLen = Len % 3;
            rtn += Mid(tmp1, 1, fLen);
            for (c = fLen + 1; c <= Len; c += 3)
            {
                if (rtn != ("")) rtn += ",";
                rtn += Mid(tmp1, c, c + 3 - 1);
            };
            rtn += tmp2;
	        if(cConvert.ToDouble(rtn) != 0) rtn = fg + rtn;
	        return rtn;
        }

        public static double Round(double pdNo1, int lPos = 0, int lOpt = 1)
        {
            long i = 0;
            double dPara = pdNo1;
            long lP = lPos < 0 ? -lPos : lPos;
            double dB = lPos < 0 ? 0.1 : 10;
            double dA = pdNo1 < 0 ? -1 : 1;

            for (i = 1; i <= lP; i++) dPara *= dB;     //  1000000

            if (lOpt == 1) dPara += dA * 0.9;   // 올림
            else if (lOpt == 2) dPara += dA * 0.5; // 반올림

            // 소수점킽을 띠어낸다.
            //dPara=(long)dPara;
            if (dPara >= 0)
                dPara = Math.Floor(dPara);
            else
                dPara = Math.Ceiling(dPara);

            for (i = 1; i <= lP; i++) dPara /= dB;     //  
            return dPara;
        }

        public static int GetPos(string Src, string Deli, string Desti)
        {
            return cUtil.GetPos(Src, Deli, Desti);
        }

        public static bool IsAlNum(string Src, int dig = 0)
        {
            if (dig != 0 && Src.Length != dig) return false;
            return cUtil.IsLetterOrNumber(Src);
        }

        public static bool IsAlpha(string Src, int dig = 0)
        {
            if (dig != 0 && Src.Length != dig) return false;
            return cUtil.IsLetter(Src);
        }

        public static bool IsDigit(string Src, int dig = 0)
        {
            if (dig != 0 && Src.Length != dig) return false;
            return cUtil.IsNumber(Src);
        }

        public static string SetFormat(string bsStr, bool blnDirection, int iLen)
        {
            string bsRet = "";
            int iStrLen, iGap;
            //
            iStrLen = cUtil.HLen(bsStr);      //4
            iGap = iLen - iStrLen;            //12 - 4  =>>8	

            if (iGap < 0) bsRet = cString.Mid(bsStr, 1, iLen);
            if (iGap == 0) bsRet = bsStr;
            if (iGap > 0)
            {
                bsRet = bsStr;
                if (blnDirection == false)
                {
                    for (int j = 1; j <= iGap; j++)
                    {
                        bsRet += " ";
                    }
                }
                else
                {
                    for (int j = 1; j < (iGap + 1); j++)
                    {
                        bsRet = " " + bsRet;
                    }
                }
            }
            return bsRet;
        }

        public static string SetFormat2(string bsStr, bool blnDirection, int iLen)
        {
            string bsRet = "";
            int iStrLen, iGap;

            iStrLen = cUtil.HLen(bsStr);      //4
            iGap = iLen - iStrLen;            //12 - 4  =>>8	

            if (iGap < 0) bsRet = cString.Mid(bsStr, 1, iLen);
            if (iGap == 0) bsRet = bsStr;
            if (iGap > 0)
            {
                bsRet = bsStr;
                if (blnDirection == false)
                {
                    for (int j = 1; j <= iGap; j++)
                    {
                        bsRet += " ";
                    }
                }
                else
                {
                    for (int j = 1; j < (iGap + 1); j++)
                    {
                        bsRet = " " + bsRet;
                    }
                }
            }
            return bsRet;
        }

        //'***********************************************************************************
        //'함수 설명 - MS SQL Server에 있는 PAD(LPAD & RPAD) 함수 구현
        //'인자 설명 -  vVal : 입력값 (문자나 숫자 상관 없슴)
        //'             iLen : 자리수
        //'             sChr : 빈자리에 들어갈 문자
        //'참고 사항 - 한글도 1byte로 계산 함
        //'            2Byte로 계산하고 싶다면 LenB(StrConv(vVal, vbFromUnicode)) 사용
        //'***********************************************************************************
        public static string RPAD(string vVal, int iLen, string sChr)
        {
            int iLendefault = vVal.Length;

            //'한글을 1Byte로 처리하고 싶을 때
            if (iLen <= iLendefault)
            {
                return Mid(vVal, 1, iLen);
            }
            else
            {
                string RPAD = "";
                int iLentmp = iLen - iLendefault;
                for (int i = 1; i <= iLentmp; i++)
                {
                    RPAD = RPAD + sChr;
                }

                return vVal + RPAD;
            }
        }

        //'***********************************************************************************
        //'함수 설명 - MS SQL Server에 있는 PAD(LPAD & RPAD) 함수 구현
        //'인자 설명 -  vVal : 입력값 (문자나 숫자 상관 없슴)
        //'             iLen : 자리수
        //'             sChr : 빈자리에 들어갈 문자
        //'참고 사항 - 한글도 1byte로 계산 함
        //'            2Byte로 계산하고 싶다면 LenB(StrConv(vVal, vbFromUnicode)) 사용
        //'***********************************************************************************
        public static string LPAD(string vVal, int iLen, string sChr)
        {
            int iLendefault = vVal.Length;

            //'한글을 1Byte로 처리하고 싶을 때
            if (iLen <= iLendefault)
            {
                return Mid(vVal, 1, iLen);
            }
            else
            {
                string LPAD = "";
                int iLentmp = iLen - iLendefault;
                for (int i = 1; i <= iLentmp; i++)
                {
                    LPAD = LPAD + sChr;
                }

                return LPAD + vVal;
            }
        }

        public static string ufAdjustPersions(string psValue)
        {
            if (psValue == "")
            {
                return "";
            }
            string[] arrS = (psValue).Split('.');
            string sValue = "";
            sValue = arrS[0];

            //if (arrS.Length == 2 && int.Parse(arrS[1]) > 0)
            if (arrS.Length == 2 )
            {
                for (int ii = arrS[1].Length - 1; ii >= 0; ii--)
                {
                    if (arrS[1].Substring(ii, 1) != "0")
                    {
                        sValue += "." + arrS[1].Substring(0, ii + 1);
                        break;
                    }
                }
            }
            return sValue;
        }

        public static  decimal ceil(decimal d )
        {
            return Math.Ceiling(d);
        }

        public static double ceil(double  a)
        {
            return Math.Ceiling(a);
        }
    }
}
