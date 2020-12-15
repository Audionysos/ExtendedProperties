using System;
using System.Collections.Generic;
using System.Text;

namespace com.audionysos.general.extensions
{
    public static class TypeExtensions {
        private static Dictionary<string, string> operators = new Dictionary<string, string>() {
            {"+", "op_Addition" },
            {"-", "op_Subtraction" },
            {"*", "op_Multiply" },
            {"/", "op_Division" },
            {"%", "op_Modulus" },
            {"^", "op_ExclusiveOr" },
            {"&", "op_BitwiseAnd" },
            {"|", "op_BitwiseOr" },
            {">", "op_GreaterThan" },
            {"<", "op_LessThan" },
            {"==", "op_Equality" }
        };
        /// <summary>Check if given type has specified operator (Not all supported right now).</summary>
        /// <param name="op">Opertor representation as string.</param>
        public static bool hasOperator(this Type t, string op){
            var o = t.GetMethod(operators[op]);
            return o != null && o.IsSpecialName;
        }

        /// <summary>Check if this type has all of listed operators.</summary>
        public static bool hasOperators(this Type t, params string[] ops){
            for (int i = 0; i < ops.Length; i++)
                if (!t.hasOperator(ops[i])) return false;
            return true;
        }
    }
}
