using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace XinjingDaily.Bot.Entry.Entries.Context;

[SugarTable("command_context", TableDescription = "命令上下文")]
public sealed record CommandContext
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }


    public int UserId { get; set; }

    
}
