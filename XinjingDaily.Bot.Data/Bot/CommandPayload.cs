using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Telegram.Bot.Types.Enums;

namespace XinjingDaily.Bot.Data.Bot;

public sealed record CommandPayload(MethodInfo Method, Dictionary<ChatType, string> Right);
