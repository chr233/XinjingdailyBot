using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace XinjingdailyBot.Generator;
internal static class RegexUtils
{
    static readonly Regex MatchNameSpace = new Regex(@"^namespace\s+([^;]+);");

}
