using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Data;
using System.Drawing;


namespace SynthesisOfDM
{
    class PreoFTMTX
    {
        const string KONN = "*";
        const string DIZN = "+";
        const string PLUS2 = "⊕";
        const string OTR = "!";
        const string EQU = "~";
        const string IMPL1 = "|";
        const string IMPL2 = "⇒";

        public PreoFTMTX(PreoF form3, bool fp)
        {
            f_form3 = form3;    // Форма окна программы
            form_preo = fp;     // форма преобразования (true - dnf, false - knf)
        }

        public void ResetAll()
        {
            for (int i = 0; i < 10000; ++i)
            {
                t_var[i] = "";
                f_var[i] = "";
            }

            count_p = 0;
            count_record = 0;
            bracket_count = 0;
        }

        public void ResetAllWOT()
        {
            for (int i = 0; i < 10000; ++i)
                f_var[i] = "";

            count_p = 0;
            count_record = 0;
            bracket_count = 0;
        }

        public void AddTVar(string formula, ref int ppv)
        {
            bool record = false;
            int bc = 0;
            string dump_t = "";

            for (int i = 0; i < formula.Length; ++i)
            {
                string var_f = formula[i].ToString();

                if (var_f == IMPL2)
                    var_f = IMPL1;

                if (var_f == "≡")
                    var_f = EQU;

                if (var_f == IMPL1 || var_f == KONN || var_f == DIZN || var_f == PLUS2 || var_f == EQU || var_f == ")")
                {
                    if (record)
                    {
                        record = false;
                        AddVarCheckVar(dump_t, ref ppv);
                        dump_t = "";
                    }
                    continue;
                }

                switch (var_f)
                {
                    case "#": record = true; continue;
                    case "(":
                        if (++bc > bracket_count)
                            bracket_count = bc;
                        continue;
                    case ")": --bc; break;
                    case "\r":
                    case "\n":
                        continue;
                    default:
                        if (var_f != " " && record)
                            dump_t += var_f;
                    break;
                }

                if (i + 1 == formula.Length && record)
                {
                    record = false;
                    AddVarCheckVar(dump_t, ref ppv);
                    dump_t = "";
                }
            }
        }

        public void AddVar(string formula, int ppv)
        {
            bool record = false;
            int bc = 0;
            string dump_t = "";

            for (int i = 0; i < formula.Length; ++i)
            {
                string var_f = formula[i].ToString();

                if (var_f == IMPL2)
                    var_f = IMPL1;

                if (var_f == "≡")
                    var_f = EQU;

                if (var_f == IMPL1 || var_f == KONN || var_f == DIZN || var_f == PLUS2 || var_f == EQU || var_f == ")")
                {
                    if (record)
                    {
                        record = false;
                        AddVarCheckVar(dump_t, ref ppv);
                        dump_t = "";
                    }
                    f_var[count_record++] = var_f;
                    continue;
                }

                switch (var_f)
                {
                    case "#": record = true; continue;
                    case OTR: f_var[count_record++] = var_f; continue;
                    case "(":
                        if (++bc > bracket_count)
                            bracket_count = bc;
                        f_var[count_record++] = var_f;
                        continue;
                    case ")": --bc; break;
                    case "\r":
                    case "\n":
                        continue;
                    default:
                        if (var_f != " " && record)
                            dump_t += var_f;
                        break;
                }

                if (i + 1 == formula.Length && record)
                {
                    record = false;
                    AddVarCheckVar(dump_t, ref ppv);
                    dump_t = "";
                }
                count_p = ppv;
            }

            msg_about_f_var("Обработка формулы окончена.", false);
        }

        public void AddVarCheckVar(string dump, ref int cr_c)
        {
            bool check_t = false;
            for (int j = 0; j < cr_c; ++j)
            {
                if (t_var[j] == dump)
                {
                    f_var[count_record++] = j.ToString();
                    check_t = true;
                    break;
                }
            }

            if (!check_t)
            {
                f_var[count_record++] = cr_c.ToString();
                t_var[cr_c++] = dump;
            }
        }

        public bool CheckFormlTr()
        {
            int count_l_br = 0;
            int count_r_br = 0;

            for (int i = 0; i < count_record; ++i)
            {
                if (f_var[i] == "(")
                    ++count_l_br;

                if (f_var[i] == ")")
                    ++count_r_br;

                bool[] tr_doubl_sys_el = new bool[3];
                for (int b = -1; b < 2; ++b)
                {
                    if (i + b < 0)
                        break;

                    switch (f_var[i + b])
                    {
                        case KONN:
                        case DIZN:
                        case PLUS2:
                        case EQU:
                        case IMPL1:
                        case IMPL2:
                            if (i + b == 0)
                                return false;

                            tr_doubl_sys_el[b + 1] = true;
                            break;
                    }
                }

                if (tr_doubl_sys_el[1] && (tr_doubl_sys_el[0] || tr_doubl_sys_el[2]))
                    return false;
            }

            if (count_l_br != count_r_br)
                return false;

            return true;
        }

        public bool Preo_obr()
        {
            /* Этап 1:  Превращение эквивалентности в сложение по модулю и превращение сложения по модулю в сложение */
            bool h_que_or_m = true;
            for (int ff = 0; ff < 2; ++ff)
            {
                h_que_or_m = true;
                while (h_que_or_m)
                {
                    h_que_or_m = false;
                    for (int j = 0; j < count_record; ++j)
                    {
                        if (f_var[j] == "")
                            break;

                        if (ff == 0 && f_var[j] == EQU || ff == 1 && f_var[j] == PLUS2)
                            h_que_or_m = true;
                    }

                    if (!h_que_or_m)
                        break;

                    if (ff == 0)
                    {
                        if (!PreoEquival())
                            return false;
                    }
                    else
                    {
                        if (!PreoModulo())
                            return false;
                    }

                    PreoSmartDelBracket();
                    Preo_obr_1_m1();
                }
                msg_about_f_var("Преобразование:", false);
            }

            /* Этап 2: Превращение импликации в сложение */
            for (int j = 0; j < count_record; ++j)
            {
                if (f_var[j] == "")
                    break;

                if (f_var[j] == IMPL1)
                {
                    string[] param1 = new string[count_record];
                    int start_s = PreoModulo_int(j, true, ref param1, false, false);

                    f_var[j] = DIZN;

                    for (int ff = 0; ff < 2; ++ff)
                        if (!Preo_sdv(start_s, false))
                            return false;

                    f_var[start_s] = "!";
                    f_var[start_s + 1] = "(";
                    j += 2;

                    if (!Preo_sdv(j, false))
                        return false;

                    f_var[j++] = ")";
                }
            }
            PreoSmartDelBracket();
            Preo_obr_1_m1();
            msg_about_f_var("Преобразование:", false);

            /* Этап 3: Вхождение отрицания в скобку */
            h_que_or_m = true;
            while (h_que_or_m)
            {
                for (int i = bracket_count; i >= 0; --i)
                {
                    int count_s = 0;
                    bool tr_bl = false;
                    for (int j = 1; j < count_record; ++j)
                    {
                        if (f_var[j] == "")
                            break;

                        if (f_var[j] == "(" && ++count_s == i && f_var[j - 1] == OTR)
                        {
                            tr_bl = true;
                            f_var[j - 1] = "-1";
                        }

                        if (f_var[j] == ")" && i == count_s--)
                            tr_bl = false;

                        if (tr_bl && i == count_s)
                        {
                            if (f_var[j] == "(" || f_var[j] == DIZN || f_var[j] == KONN)
                            {
                                switch (f_var[j])
                                {
                                    case KONN: f_var[j] = DIZN; break;
                                    case DIZN: f_var[j] = KONN; break;
                                }

                                string[] param2 = new string[count_record];
                                int end_s = PreoModulo_int(j, false, ref param2, false, false) + 1;

                                if (param2.Length <= 2 || (f_var[end_s] == ")" && f_var[j] != KONN))
                                {
                                    if (!Preo_sdv(j + 1, false))
                                        return false;
                                    f_var[++j] = "!";
                                }
                                else
                                {
                                    if (!Preo_sdv(end_s, false))
                                        return false;

                                    f_var[end_s] = ")";

                                    for (int ff = 0; ff < 2; ++ff)
                                        if (!Preo_sdv(j + 1, false))
                                            return false;

                                    f_var[++j] = "!";
                                    f_var[++j] = "(";
                                    ++count_s;
                                }
                            }
                        }
                    }
                    Preo_obr_1_m1();
                }

                h_que_or_m = false;
                for (int i = 0; i < count_record; ++i)
                {
                    if (f_var[i] == OTR && f_var[i + 1] == "(")
                        h_que_or_m = true; // Повторяем до тех пор, пока есть отрицание перед скобкой
                }

                Preo_obr_1_m1();
                Preo_obr_d();
                PreoSmartDelBracket();
                msg_about_f_var("Преобразование:", false);
            }

            if (form_preo)
            {
                if (!PreoDNF())
                    return false;
            }
            else
            {
                if (!PreoKNF())
                    return false;
            }

            msg_about_f_var("Преобразование (финал):", false);
            return true;
        }

        public void PreoDelDoubleVar()
        {
            // Этап 4: Удаление повторных переменных
            // Примечание: у форм преобразований разный способ удаления переменных
            // "!a*a"   ->   ""
            // "a*a"    ->   "a"
            // "!a*!a"  ->   "!a"
            // "a+a"    ->   "a"
            bool the_end = false;
            while (!the_end && count_record != 0)
            {
                PreoDelDoubleVar_Calc(ref the_end);
                Preo_obr_1_m1();
                PreoSmartDelBracket();
            }
        }

        public void PreoDelDoubleVar_Calc(ref bool the_end)
        {
            the_end = true;
            Preo_obr_1_m1();
            PreoSmartDelBracket();

            for (int i = bracket_count; i >= 0; --i)
            {
                int count_s = 0;
                int start_d = 0;

                for (int j = 0; j < count_record; ++j)
                {
                    if (f_var[j] == "(" && ++count_s == i)
                        start_d = j + 1;

                    if (f_var[j] == ")")
                        --count_s;

                    if (i == count_s)
                    {
                        if (f_var[j] == DIZN)
                            start_d = j + 1;

                        if (CanConvertTo(f_var[j]))
                        {
                            int count_sh = count_s;
                            for (int h = start_d; h < count_record; ++h)
                            {
                                if (f_var[h] == "(")
                                    ++count_sh;

                                if (f_var[h] == ")" && count_sh-- == count_s)
                                    break;

                                if (count_sh != count_s)
                                    continue;

                                if (f_var[h] == DIZN)
                                    break;

                                if (j != h && f_var[j] == f_var[h])
                                {
                                    the_end = false;
                                    bool j_otr = (j > 0 && f_var[j - 1] == OTR);
                                    bool h_otr = (h > 0 && f_var[h - 1] == OTR);

                                    if (j_otr != h_otr)
                                    {
                                        string[] right_param = new string[count_record];
                                        int end_s = PreoModulo_int(j + 1, false, ref right_param, false, false);

                                        // ::TEST FUNCTION:: //
                                        if (start_d > 0 && f_var[start_d - 1] == "(" && end_s + 1 < count_record && f_var[end_s + 1] == ")")
                                            if (start_d > 1 && f_var[start_d - 2] == KONN
                                                || end_s + 2 < count_record && f_var[end_s + 2] == KONN)
                                            {
                                                the_end = true;
                                                return;
                                            }

                                        for (int g = start_d; g <= end_s; ++g)
                                            f_var[g] = "-1";
                                        Preo_obr_1_m1();

                                        if (start_d == 0 || f_var[start_d] == KONN)
                                        {
                                            f_var[start_d] = "-1";
                                            return;
                                        }

                                        if (start_d > 0 && f_var[start_d - 1] == KONN)
                                        {
                                            f_var[start_d - 1] = "-1";
                                            return;
                                        }

                                        if (f_var[start_d] == DIZN)
                                        {
                                            f_var[start_d] = "-1";
                                            return;
                                        }

                                        if (start_d > 0 && f_var[start_d - 1] == DIZN)
                                        {
                                            f_var[start_d - 1] = "-1";
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        f_var[j] = "-1";
                                        int pos_act_left = j - 1;
                                        if (j_otr)
                                        {
                                            if (j > 1)
                                                pos_act_left = j - 2;
                                            f_var[j - 1] = "-1";
                                        }

                                        if (pos_act_left < 0)
                                            pos_act_left = j;

                                        if (f_var[pos_act_left] == KONN)
                                        {
                                            f_var[pos_act_left] = "-1";
                                            return;
                                        }

                                        if (ValueLCountRecord(j) && f_var[j + 1] == KONN)
                                        {
                                            f_var[j + 1] = "-1";
                                            return;
                                        }

                                        if (f_var[pos_act_left] == DIZN)
                                        {
                                            f_var[pos_act_left] = "-1";
                                            return;
                                        }

                                        if (ValueLCountRecord(j) && f_var[j + 1] == DIZN)
                                        {
                                            f_var[j + 1] = "-1";
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public int PreoSmartFindFirstS(int var_st_pos, int step)
        {
            for (int i = var_st_pos; ; i += step)
            {
                if (i == 0 || i == count_record)
                    break;

                if (f_var[i] == "-1" || f_var[i] == "")
                    continue;

                return i;
            }

            return -1;
        }

        public bool PreoEquival()
        {
            PreoSmartDelBracket();
            Preo_obr_1_m1();

            // Преобразование знака ≡//
            string[] param1 = new string[count_record];
            string[] param2 = new string[count_record];

            for (int i = bracket_count; i >= 0; --i)
            {
                int count_s = 0;
                bool true_obr = false;
                for (int j = 0; j < count_record; ++j)
                {
                    if (f_var[j] == "")
                        break;

                    if (i == 0 || f_var[j] == "(" && ++count_s == i)
                        true_obr = true;

                    if (f_var[j] == ")" && i == count_s--)
                        true_obr = false;

                    if (j == 0 || !true_obr)
                        continue;

                    if (f_var[j] == EQU)
                    {
                        int start_s = PreoModulo_int(j, true, ref param1, true, false);
                        int end_s = PreoModulo_int(j, false, ref param2, true, false);

                        string strgfd = "";
                        for (int h = 0; h < param2.Length; ++h)
                            strgfd += param2[h];

                        for (int f = start_s; f < end_s; ++f)
                            f_var[f] = "-1";
                        j = end_s + 1;

                        string[] all_p = new string[(param1.Length + param2.Length) * 2 + 100];
                        int counter_ap = 0;

                        all_p[counter_ap++] = "(";
                        if (param1.Length > 2)
                            all_p[counter_ap++] = "(";
                        for (int e = param1.Length - 1; e >= 0; --e)
                            if (param1[e] != "-1")
                                all_p[counter_ap++] = param1[e];
                        if (param1.Length > 2)
                            all_p[counter_ap++] = ")";

                        all_p[counter_ap++] = IMPL1;
                        for (int e = 0; e < param2.Length; ++e)
                            if (param2[e] != "-1")
                                all_p[counter_ap++] = param2[e];
                        all_p[counter_ap++] = ")";
                        all_p[counter_ap++] = KONN;
                        all_p[counter_ap++] = "(";

                        if (param2.Length > 2)
                            all_p[counter_ap++] = "(";
                        for (int e = 0; e < param2.Length; ++e)
                            if (param2[e] != "-1")
                                all_p[counter_ap++] = param2[e];
                        if (param2.Length > 2)
                            all_p[counter_ap++] = ")";

                        all_p[counter_ap++] = IMPL1;
                        for (int e = param1.Length - 1; e >= 0; --e)
                            if (param1[e] != "-1")
                                all_p[counter_ap++] = param1[e];
                        all_p[counter_ap++] = ")";

                        Preo_obr_1_m1();
                        for (int j2 = start_s; j2 < start_s + counter_ap - 1; ++j2)
                            if (!Preo_sdv(start_s, false))
                                return false;

                        int counter_s2 = 0;
                        for (int j2 = start_s; j2 < start_s + counter_ap; ++j2)
                            f_var[j2] = all_p[counter_s2++];

                        param1 = new string[count_record];
                        param2 = new string[count_record];
                    }
                }
            }
            return true;
        }

        public bool PreoModulo()
        {
            PreoSmartDelBracket();
            Preo_obr_1_m1();

            // Преобразование знака ⊕ //
            string[] param1 = new string[count_record];
            string[] param2 = new string[count_record];

            for (int i = bracket_count; i >= 0; --i)
            {
                int count_s = 0;
                bool true_obr = false;
                for (int j = 0; j < count_record; ++j)
                {
                    if (f_var[j] == "")
                        break;

                    if (i == 0 || f_var[j] == "(" && ++count_s == i)
                        true_obr = true;

                    if (f_var[j] == ")" && i == count_s--)
                        true_obr = false;

                    if (j == 0 || !true_obr)
                        continue;

                    if (f_var[j] == PLUS2)
                    {
                        int start_s = PreoModulo_int(j, true, ref param1, true, false);
                        int end_s = PreoModulo_int(j, false, ref param2, true, false);

                        string param1_s = "";
                        string param2_s = "";
                        for (int w = 0; w < param1.Length; ++w)
                            param1_s += param1[w];

                        for (int w = 0; w < param2.Length; ++w)
                            param2_s += param2[w];

                        for (int f = start_s; f <= end_s; ++f)
                            f_var[f] = "-1";
                        j = end_s + 1;

                        if (param1.Length > 0 && param1[param1.Length - 1] == DIZN)
                        {
                            ++start_s;
                            ++end_s;
                        }

                        string[] all_p = new string[(param1.Length + param2.Length) * 2 + 16];
                        int counter_ap = 0;

                        for (int g = 0; g < 2; ++g)
                        {
                            if (g == 0)
                                all_p[counter_ap++] = OTR;

                            if (param1.Length > 2)
                                all_p[counter_ap++] = "(";

                            for (int e = param1.Length - 1; e >= 0; --e)
                                if (param1[e] != "-1")
                                    all_p[counter_ap++] = param1[e];

                            if (param1.Length > 2)
                                all_p[counter_ap++] = ")";

                            all_p[counter_ap++] = KONN;

                            if (g == 1)
                                all_p[counter_ap++] = OTR;

                            if (param2.Length > 2)
                                all_p[counter_ap++] = "(";

                            for (int e = 0; e < param2.Length; ++e)
                                if (param2[e] != "-1")
                                    all_p[counter_ap++] = param2[e];

                            if (param2.Length > 2)
                                all_p[counter_ap++] = ")";

                            if (g == 0)
                                all_p[counter_ap++] = DIZN;
                        }

                        Preo_obr_1_m1();
                        for (int j2 = start_s; j2 < start_s + counter_ap; ++j2)
                            if (!Preo_sdv(start_s, false))
                                return false;

                        int counter_s2 = 0;
                        for (int j2 = start_s; j2 < start_s + counter_ap; ++j2)
                            f_var[j2] = all_p[counter_s2++];

                        param1 = new string[count_record];
                        param2 = new string[count_record];

                        string asdas2d = "";
                        for (int f = 0; f < counter_ap; ++f)
                            asdas2d += all_p[f];

                        asdas2d += Environment.NewLine;

                        for (int f = 0; f < count_record; ++f)
                            asdas2d += f_var[f];
                    }
                }
            }
            PreoSmartDelBracket();
            Preo_obr_1_m1();
            return true;
        }

        public int PreoModulo_int(int pos_fvar, bool left, ref string[] param, bool equiv_dizn, bool DNF_preo)
        {
            int count_s = 0;
            int int_return = 0;
            int counter_p = 0;

            for (int i = 0; i < param.Length; ++i)
                param[i] = "";

            if (left)
            {
                for (int j = pos_fvar - 1; j >= 0; --j)
                {
                    if (f_var[j] == "(" && count_s++ == 0)
                        break;

                    if (f_var[j] == ")" && --count_s == 0)
                        break;

                    if (count_s == 0)
                    {
                        if (!equiv_dizn && f_var[j] == DIZN || f_var[j] == IMPL1 || f_var[j] == PLUS2 || f_var[j] == EQU)
                            break;

                        if (DNF_preo && f_var[j] == KONN)
                            break;
                    }

                    int_return = j;
                    param[counter_p++] = f_var[j];
                }
            }
            else
            {
                for (int j = pos_fvar + 1; j < count_record; ++j)
                {
                    if (f_var[j] == "")
                        break;

                    if (f_var[j] == "(" && ++count_s == 0)
                        break;

                    if (f_var[j] == ")" && count_s-- == 0)
                        break;

                    if (count_s == 0)
                    {
                        if (!equiv_dizn && f_var[j] == DIZN || f_var[j] == IMPL1 || f_var[j] == PLUS2 || f_var[j] == EQU)
                            break;

                        if (DNF_preo && f_var[j] == KONN)
                            break;
                    }

                    int_return = j;
                    param[counter_p++] = f_var[j];
                }
            }

            string[] param_d = new string[counter_p];
            for (int i = 0; i < counter_p; ++i)
                param_d[i] = param[i];
            param = param_d;

            return int_return;
        }

        public bool PreoDNF()
        {
            /* Этап 2: упрощение скобок */
            //------------------------
            // Удаление лишних скобок
            PreoSmartDelBracket();
            Preo_obr_1_m1();

            bool repeat = true;
            while (repeat)
            {
                for (int j = 0; j < count_record; ++j)
                {
                    if (j >= max_f_var_value)
                        return false;

                    if (f_var[j] == "")
                        break;

                    if (f_var[j] == KONN)
                    {
                        bool l_br = (f_var[j - 1] == ")");
                        bool r_br = (f_var[j + 1] == "(");

                        if (!l_br && !r_br)
                            continue;

                        string[] param1 = new string[count_record];
                        string[] param2 = new string[count_record];
                        string[] new_dz = new string[1000000]; int i_new_dz = 0;
                        int start_s = PreoModulo_int(j, true, ref param1, false, true);
                        int end_s = PreoModulo_int(j, false, ref param2, false, true);

                        for (int f = start_s; f < end_s; ++f)
                            f_var[f] = "-1";

                        int br_count = 0;
                        bool was_b_plus = false;
                        if (l_br)
                        {
                            // Если слева от умножения стоит скобка
                            for (int g = param1.Length - 1; g >= 0; --g)
                            {
                                if (i_new_dz + 1 > new_dz.Length)
                                    Array.Resize<string>(ref new_dz, i_new_dz + 1);
                                new_dz[i_new_dz++] = param1[g];

                                if (param1[g] == "(")
                                    ++br_count;

                                if (param1[g] == ")")
                                    --br_count;

                                if (br_count != 1)
                                    continue;

                                if (param1[g] == DIZN)
                                    was_b_plus = false;

                                if (was_b_plus)
                                    continue;

                                // Ошибка возникла после добавления param2[g] == ")"; продумать возможные ошибки
                                if (CanConvertTo(param1[g]) || param1[g] == ")")
                                {
                                    if (i_new_dz + param2.Length + 2 > new_dz.Length)
                                        Array.Resize<string>(ref new_dz, i_new_dz + param2.Length + 2);

                                    was_b_plus = true;
                                    new_dz[i_new_dz++] = KONN;
                                    for (int g2 = 0; g2 < param2.Length; ++g2)
                                        new_dz[i_new_dz++] = param2[g2];
                                }
                            }
                        }
                        else
                        {
                            // Если слева от умножения нет скобки
                            for (int g = 0; g < param2.Length; ++g)
                            {
                                if (i_new_dz + 1 > new_dz.Length)
                                    Array.Resize<string>(ref new_dz, i_new_dz + 1);
                                new_dz[i_new_dz++] = param2[g];

                                if (param2[g] == "(")
                                    ++br_count;

                                if (param2[g] == ")")
                                    --br_count;

                                if (br_count != 1)
                                    continue;

                                if (param2[g] == DIZN)
                                    was_b_plus = false;

                                if (was_b_plus)
                                    continue;

                                // Ошибка возникла после добавления param2[g] == ")"; продумать возможные ошибки
                                if (CanConvertTo(param2[g]) || param2[g] == ")")
                                {
                                    if (i_new_dz + param1.Length + 2 > new_dz.Length)
                                        Array.Resize<string>(ref new_dz, i_new_dz + param1.Length + 2);

                                    was_b_plus = true;
                                    new_dz[i_new_dz++] = KONN;
                                    for (int g2 = param1.Length - 1; g2 >= 0; --g2)
                                        new_dz[i_new_dz++] = param1[g2];
                                }
                            }
                        }

                        Preo_obr_1_m1();
                        for (int j2 = start_s; j2 < start_s + i_new_dz - 1; ++j2)
                            if (!Preo_sdv(start_s, false))
                                return false;

                        int counter_s2 = 0;
                        for (int j2 = start_s; j2 < start_s + i_new_dz; ++j2)
                            f_var[j2] = new_dz[counter_s2++];
                        j = start_s + i_new_dz;

                        if (l_br && r_br)
                            break;
                    }
                }
                PreoDelDoubleVar();

                repeat = false;
                for (int j = 0; j < count_record; ++j)
                {

                    if (f_var[j] == KONN && (j > 0 && f_var[j - 1] == ")" || j + 1 < count_record && f_var[j + 1] == "("))
                        repeat = true;
                }

                msg_about_f_var("Преобразование ДНФ: ", false);
            }

            PreoDelDoubleVar();
            return true;
        }

        public int PreoSmartDelBracket()
        {
            // Функция удаления ненужных (лишних) скобок //
            SetNewBracketCount();
            for (int i = bracket_count; i >= 0; --i)
            {
                int count_s = 0;
                int bracket_st_del = -1;
                bool was_dizn = false;
                bool tr_bracket = false;

                for (int j = 0; j < count_record; ++j)
                {
                    if (f_var[j] == "")
                        break;

                    if (f_var[j] == "(" && i == ++count_s)
                    {
                        bracket_st_del = j;
                        tr_bracket = true;
                    }

                    if (f_var[j] == ")" && i == count_s--)
                    {
                        if (!was_dizn && bracket_st_del != -1)
                        {
                            if (bracket_st_del > 0 && f_var[bracket_st_del - 1] == OTR)
                                continue;

                            f_var[bracket_st_del] = "-1";
                            f_var[j] = "-1";


                            if (bracket_st_del + 1 == j)
                                PreoSmartDelBracket_action(bracket_st_del, j);
                        }
                    }

                    if (tr_bracket && i == count_s)
                    {
                        switch (f_var[j])
                        {
                            case DIZN:
                            case PLUS2:
                            case EQU:
                            case IMPL1:
                                was_dizn = true;
                                break;
                        }
                    }
                }
            }
            SetNewBracketCount();

            for (int i = bracket_count; i >= 0; --i)
            {
                int count_s = 0;
                int bracket_st_del = -1;
                bool was_plus2 = false;
                bool was_equ = false;
                bool tr_bracket = false;
                for (int j = 0; j < count_record; ++j)
                {
                    if (f_var[j] == "")
                        break;

                    if (f_var[j] == "(" && i == ++count_s)
                    {
                        if (j == 0 || PreoSmartDelBracket_sw(j - 1))
                            bracket_st_del = j;

                        if (f_var[j + 1] == ")")
                            bracket_st_del = j;

                        tr_bracket = true;
                    }

                    if (f_var[j] == ")" && i == count_s--)
                    {
                        if (bracket_st_del > 0 && f_var[bracket_st_del - 1] == OTR)
                            continue;

                        bool del_fv = false;
                        if (bracket_st_del + 1 == j)
                        {
                            del_fv = true;
                            PreoSmartDelBracket_action(bracket_st_del, j);
                        }
                        else if ((j + 1 == count_record || PreoSmartDelBracket_sw(j + 1)) && !was_plus2 && !was_equ)
                        {
                            if (bracket_st_del == -1)
                                continue;

                            del_fv = true;
                        }

                        if (del_fv)
                        {
                            f_var[bracket_st_del] = "-1";
                            f_var[j] = "-1";
                        }

                        tr_bracket = false;
                        bracket_st_del = -1;
                    }

                    if (tr_bracket)
                    {
                        if (f_var[j] == PLUS2)
                            was_plus2 = true;

                        if (f_var[j] == EQU)
                            was_equ = true;
                    }

                    if (i > count_s && f_var[j] == KONN)
                        bracket_st_del = -1;
                }
            }

            SetNewBracketCount();
            return bracket_count;
        }

        public bool PreoSmartDelBracket_sw(int pos_j)
        {
            switch (f_var[pos_j])
            {
                case DIZN:
                case PLUS2:
                case EQU:
                case "":
                case "(":
                case ")":
                    //if (pos_j > 0 && f_var[pos_j - 1] == OTR)
                    //   return false; // TODO TODO
                    return true;
            }
            return false;
        }

        public void PreoSmartDelBracket_action(int pos_1, int pos_2)
        {
            bool del = false;
            if (pos_1 > 0 && f_var[pos_1 - 1] == KONN)
            {
                del = true;
                f_var[pos_1 - 1] = "-1";
            }

            if (!del && pos_2 + 1 < count_record && f_var[pos_2 + 1] == KONN)
            {
                del = true;
                f_var[pos_2 + 1] = "-1";
            }

            if (!del && pos_1 > 0 && f_var[pos_1 - 1] == DIZN)
            {
                del = true;
                f_var[pos_1 - 1] = "-1";
            }

            if (!del && pos_2 + 1 < count_record && f_var[pos_2 + 1] == DIZN)
                f_var[pos_2 + 1] = "-1";
        }

        public void SetNewBracketCount()
        {
            Preo_obr_1_m1();

            int new_bc = 0;
            int now_bc = 0;
            for (int j = 0; j < count_record; ++j)
            {
                if (f_var[j] == "(")
                {
                    if (++now_bc > new_bc)
                        new_bc = now_bc;
                }

                if (f_var[j] == ")")
                    --now_bc;
            }
            bracket_count = new_bc;
        }

        public bool PreoKNF()
        {
            for (int i = bracket_count; i >= 0; --i)
            {
                bool in_our_bracket = false;
                int count_s = 0; // Номер скобки
                int start_s = -1; // Начало и конец скобки, в которой выполняется преобразование (координата J)
                int end_s = -1;

                PreoKNFResetAllknfn();
                aknfnpir = new int[count_record + 1]; // Сколько переменных в конъюнкте
                aknfnpir_p = new int[count_record + 1, count_record + 1]; // Сколько переменных в множителе конъюнкта
                bool was_plus = false; // Был ли вообще знак "+" и требуется ли преобразование?
                int aknfn_p = 0; // количество конструкций all_knf_n (разделены +)
                int aknfn_p_p = 0; // Индекс[2] массива all_knf_n множителя
                int aknfn_p_p_p = 0; // Индекс[3] массива all_knf_n переменной (в множителе)

                for (int j = 0; j < count_record; ++j)
                {
                    if (f_var[j] == "")
                        break;

                    if ((f_var[j] == "(" && i == ++count_s) || (j == 0 && i == 0))
                    {
                        start_s = j;
                        in_our_bracket = true; // Вошли в скобку, где происходит преобразование
                    }

                    if (f_var[j] == ")" && i == count_s--)
                    {
                        in_our_bracket = false; // Вышли из скобки, в которой творятся дела
                        end_s = j;

                        if (was_plus)
                        {
                            j = PreoKNF_R(aknfn_p, start_s, end_s);
                            was_plus = false;

                            if (j == -1)
                                return false;
                        }

                        start_s = -1;
                        end_s = -1;

                        PreoKNFResetAllknfn();
                        aknfnpir = new int[count_record + 1];
                        aknfnpir_p = new int[count_record + 1, count_record + 1];

                        aknfn_p = 0;
                        aknfn_p_p = 0;
                        aknfn_p_p_p = 0;
                    }

                    // Если действие в нашей скобке
                    if (i == count_s)
                    {
                        if (f_var[j] == DIZN)
                        {
                            // Сбор base_knf_n закончен
                            // Далее - анализ конструкций

                            was_plus = true; // Преобразование необходимо
                            aknfn_p_p = 0;
                            aknfn_p_p_p = 0;
                            ++aknfn_p;
                        }

                        if (f_var[j] == KONN)
                        {
                            aknfn_p_p_p = 0;
                            aknfnpir[aknfn_p] = ++aknfn_p_p;
                        }
                    }

                    if (in_our_bracket && CanConvertTo(f_var[j]))
                    {
                        // ВАЖНО: во время сбора переменных мы сохраняем их j, а не f_var[j]
                        // это необходимо, чтобы в дальнейшем узнать, стоит ли отрицание в f_var[j-1]
                        if (aknfn_p + 1 > all_knf_n.Length)
                        {
                            Array.Resize<int[][]>(ref all_knf_n, aknfn_p + 1);
                            all_knf_n[aknfn_p] = new int[1][];
                            all_knf_n[aknfn_p][0] = new int[1];
                        }

                        if (aknfn_p_p + 1 > all_knf_n[aknfn_p].Length)
                        {
                            Array.Resize<int[]>(ref all_knf_n[aknfn_p], aknfn_p_p + 1);
                            all_knf_n[aknfn_p][aknfn_p_p] = new int[1];
                        }

                        if (aknfn_p_p_p + 1 > all_knf_n[aknfn_p][aknfn_p_p].Length)
                        {
                            Array.Resize<int>(ref all_knf_n[aknfn_p][aknfn_p_p], aknfn_p_p_p + 1);
                            all_knf_n[aknfn_p][aknfn_p_p][aknfn_p_p_p] = -1;
                        }

                        aknfnpir_p[aknfn_p, aknfn_p_p] = aknfn_p_p_p;
                        all_knf_n[aknfn_p][aknfn_p_p][aknfn_p_p_p++] = j;
                    }
                }

                if (was_plus && PreoKNF_R(aknfn_p, start_s, end_s) == -1)
                    return false;
            }
            return true;
        }

        public int PreoKNF_R(int aknfn_p, int start_s, int end_s)
        {
            // Если по какой-то причине стартовая и конечная позиция скобки не указана, устанавливаем координаты начала и конца формулы.
            if (start_s == -1)
                start_s = 0;

            if (end_s == -1)
                end_s = count_record;

            // TODO: переписать массив new_kf (сделать динамическим)
            string[] new_kf = new string[100000]; // Итоговая форма, будет выглядеть следующим образом
            int[] aknfn_p_i = new int[aknfn_p + 1]; // Сколько переменных осталось просчитывать
            bool[] disactive = new bool[aknfn_p + 1]; // Пройден ли "отсчет" в этом конъюнкте
            int new_kf_n = 0; // числа - номера позиций переменных, связаны знаком +

            for (int j = 0; j <= aknfn_p; ++j)
                aknfn_p_i[j] = aknfnpir[j];

            while (aknfn_p_i[0] >= 0)
            {
                for (int s = 0; s <= aknfn_p; ++s)
                    disactive[s] = false;

                new_kf[new_kf_n++] = "(";
                for (int j = aknfn_p; j >= 0; --j)
                {
                    for (int d = 0; d <= aknfnpir_p[j, aknfn_p_i[j]]; ++d)
                    {
                        if (all_knf_n[j][aknfn_p_i[j]][d] > 0 && f_var[all_knf_n[j][aknfn_p_i[j]][d] - 1] == OTR)
                            new_kf[new_kf_n++] = OTR;
                        new_kf[new_kf_n++] = f_var[all_knf_n[j][aknfn_p_i[j]][d]];
                        new_kf[new_kf_n++] = DIZN;
                    }

                    if ((j == aknfn_p || disactive[j + 1]) && --aknfn_p_i[j] < 0)
                    {
                        disactive[j] = true;

                        if (j > 0)
                            aknfn_p_i[j] = aknfnpir[j];
                    }
                }
                --new_kf_n;
                new_kf[new_kf_n++] = ")";
                new_kf[new_kf_n++] = KONN;
            }
            new_kf_n -= 2; // Нужно "удалить" лишнее умножение в конце

            // Удаляем все, что в преобразуемой скобке.  
            for (int j = start_s; j <= end_s; ++j)
                f_var[j] = "-1";
            Preo_obr_1_m1();

            // Сдвигаем от start_s правую часть формулы на new_kf_n позиций
            for (int j = 0; j <= new_kf_n; ++j)
                if (!Preo_sdv(start_s, false))
                    return -1;

            int j_n = 0;
            for (int j = start_s; j <= start_s + new_kf_n; ++j)
                f_var[j] = new_kf[j_n++];

            return start_s + new_kf_n;
        }

        public void PreoKNFResetAllknfn()
        {
            all_knf_n = new int[1][][];
            all_knf_n[0] = new int[1][];
            all_knf_n[0][0] = new int[1];
        }

        public bool Preo_sdv(int position_i, bool left)
        {
            // Сдвиг на 1 пункт влево или вправо
            if (count_record > max_f_var_value)
            {
                if (MessageBox.Show("Ошибка: переполнение формулы. Вывести ошибочную формулу (большой объем)?",
                        "Ошибка расчета ДНФ", MessageBoxButtons.YesNo) == DialogResult.OK)
                    ShowNewFvar();
                return false;
            }

            if (!left)
            {
                if (count_record + 1 > f_var.Length)
                    Array.Resize<string>(ref f_var, count_record + 1);
                for (int i = count_record; i > position_i; --i)
                    if (i > 0)
                        f_var[i] = f_var[i - 1];
                ++count_record;
            }
            else
            {
                --count_record;
                for (int i = position_i; i < count_record; ++i)
                    f_var[i] = f_var[i + 1];
            }
            return true;
        }

        public void Preo_obr_1_m1()
        {
            // Удаление всего лишнего
            for (int i = 0; i < count_record; )
            {
                if (f_var[i] == "")
                    break;

                if (f_var[i] == "-1")
                    Preo_sdv(i, true);
                else ++i;
            }
        }

        public void Preo_obr_d()
        {
            // Удаление двойных отрицаний
            for (int i = 0; i < count_record; ++i)
            {
                if (i < count_record && f_var[i] == OTR && f_var[i + 1] == OTR)
                {
                    Preo_sdv(i + 1, true);
                    Preo_sdv(i, true);
                }
            }
        }

        public int GetLastBracketEnd(int count_s, int pos_var)
        {
            int pos = count_record;

            int count_gs = count_s;
            for (int i = pos_var; i < count_record; ++i)
            {
                if (f_var[i] == "(")
                    ++count_gs;

                if (f_var[i] == ")" && count_s == count_gs--)
                {
                    pos = count_record;
                    break;
                }
            }
            return pos;
        }

        public string GenMat()
        {
            msg_about_f_var("Генерация матрицы... ", false);
            string full_p = ""; // Готовая матрица
            string f_var_p = ""; // Кусок (строка) матрицы
            int pos_start = 0; // Позиция, с которой начинается поиск переменных в конъюнкте.
            int count_plus = 0; // Количество плюсов/умножений в формуле.

            if (form_preo)
            {
                for (int i = 0; i < count_record; ++i)
                    if (f_var[i] == DIZN)
                        ++count_plus;

                if (count_plus > 0)
                    ++count_plus;

                for (int g = 0; g < count_plus; ++g)
                {
                    int vrps = 0;
                    for (int j = 0; j <= count_p; ++j)
                    {
                        if (t_var[j] == "")
                            break;

                        bool true_find = false;
                        for (int i = pos_start; i < count_record; ++i)
                        {
                            if (f_var[i] == j.ToString())
                            {
                                string d_f = "";
                                if (i > 0 && f_var[i - 1] == OTR)
                                    d_f = "0";
                                else
                                    d_f = "1";

                                f_var_p += d_f;
                                true_find = true;
                            }

                            if (f_var[i] == DIZN || i + 1 == count_record)
                            {
                                vrps = i + 1;
                                break;
                            }
                        }

                        if (!true_find)
                            f_var_p += "2";
                    }

                    if (count_plus > 0)
                        full_p += f_var_p + Environment.NewLine;
                    f_var_p = "";
                    pos_start = vrps;
                }
            }
            else
            {
                // Примечание: count_plus - количество знаков умножения (лень делать еще одну переменную)
                for (int i = 0; i < count_record; ++i)
                    if (f_var[i] == KONN)
                        ++count_plus;

                if (count_plus > 0)
                    ++count_plus;

                for (int g = 0; g < count_plus; ++g)
                {
                    int vrps = 0;
                    for (int j = 0; j <= count_p; ++j)
                    {
                        if (t_var[j] == "")
                            break;

                        bool true_find = false;
                        for (int i = pos_start; i < count_record; ++i)
                        {
                            if (f_var[i] == j.ToString())
                            {
                                string d_f = "";
                                if (i > 0 && f_var[i - 1] == OTR)
                                    d_f = "0";
                                else
                                    d_f = "1";


                                f_var_p += d_f;
                                true_find = true;
                            }

                            if (f_var[i] == KONN || i + 1 == count_record)
                            {
                                vrps = i + 1;
                                break;
                            }
                        }

                        if (!true_find)
                        {
                            if (f_var_p != "")
                                f_var_p += " ";

                            f_var_p += "2";
                        }

                    }

                    if (count_plus > 0)
                        full_p += f_var_p + Environment.NewLine;
                    f_var_p = "";
                    pos_start = vrps;
                }
            }

            // Первая строка - число столбцов и строк в матрице.
            tt_var = "Матрица создана:" + Environment.NewLine;
            tt_var += "размерность: " + (1 + count_plus).ToString() + "x" + count_p;
            tt_var += Environment.NewLine + Environment.NewLine + full_p + Environment.NewLine;

            // Вывод матрицы на экран
            f_form3.AddText(tt_var);

            // Здесь находится матрица
            // full_p

            return full_p;
        }

        public void msg_about_f_var(string msg, bool matrix)
        {
            if (count_record > 5000)
                return;

            string m_b = "";
            for (int i = 0; i < count_record; ++i)
                if (f_var[i] != "-1")
                {
                    if (CanConvertTo(f_var[i]))
                        m_b += "#" + t_var[Convert.ToInt32(f_var[i])];
                    else m_b += f_var[i];
                }

            if (m_b == "")
                m_b = "решений нет.";

            string final_text = "";

            if (matrix)
            {
                final_text = "Публикация финальной формулы: " + Environment.NewLine + m_b + Environment.NewLine + Environment.NewLine + msg + Environment.NewLine;
            }
            else
            {
                final_text = msg + Environment.NewLine + m_b;
            }

            f_form3.AddText(final_text);
        }

        public void Save(bool two)
        {
            string sttwo = two ? "#" : "";
            for (int i = 0; ; ++i)
            {
                if (!System.IO.File.Exists("CTable_" + i + sttwo + ".txt"))
                {
                    System.IO.File.AppendAllText("CTable_" + i + sttwo + ".txt", tt_var);
                    break;
                }
            }

            msg_about_f_var("Публикация матрицы: " + Environment.NewLine + tt_var, true);
        }

        public bool CanConvertTo(string ms)
        {
            try
            {
                int d = Convert.ToInt32(ms);
                if (d == -1)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string ConvertArrayToString(string[] array)
        {
            string result = "";
            for (int i = 0; i < array.Length; ++i)
                result += array[i];

            return result;
        }

        public void ShowNewFvar()
        {
            string new_f_var = "";
            for (int i = 0; i < count_record; ++i)
            {
                if (i >= max_f_var_value)
                {
                    new_f_var += "...";
                    break;
                }                    

                new_f_var += f_var[i];
            }

            f_form3.AddText(new_f_var);
        }

        public string GetResultTT() { return tt_var; }
        public bool ValueLCountRecord(int value) { return (value + 1 < count_record); }

        private int[] aknfnpir;
        private int[,] aknfnpir_p;
        private int[][][] all_knf_n;

        int count_record;
        private string tt_var = "";

        const int max_f_var_value = 100000000;
        private string[] t_var = new string[10000];
        private string[] f_var = new string[10000];

        private int count_p;
        private int bracket_count = 0;


        private PreoF f_form3;
        private bool form_preo; // 0 - к.н.ф., 1 - д.н.ф.
        private bool true_load; // Корректно ли прогружена формула?
    }
}
