# Xinjingdaily Bot

环境要求:

- .net 6
- mysql

使用方法:

运行 Xinjingdaily, 编辑 config.json

```json
{
  "Debug": false,
  "LogLevel": 0,
  "BotToken": "",
  "Proxy": "",
  "DBHost": "127.0.0.1",
  "DBPort": 3306,
  "DBName": "xjb_db",
  "DBUser": "root",
  "DBPassword": "123456",
  "ReviewGroup": "", //@开头为公开频道,也可以为数字(非公开群组)
  "SubGroup": "", //@开头为公开频道,也可以为数字(非公开群组)
  "AcceptChannel": "",
  "RejectChannel": ""
}
```
