#!/bin/sh

cd /app

rm -f xjb_db.db appsettings.json

ln -s ./data/xjb_db.db  ./

if [ -d "./data/appsettings.json" ] ;then
    ln -s ./data/appsettings.json ./
else
    cp appsettings.template.json appsettings.json
    sed -i -e "s|<BotToken>|$BotToken|g" \
        -e "s|<SuperAdmins>|$SuperAdmins|g" \
        -e "s|<ReviewGroup>|$ReviewGroup|g" \
        -e "s|<CommentGroup>|$CommentGroup|g" \
        -e "s|<SubGroup>|$SubGroup|g" \
        -e "s|<AcceptChannel>|$AcceptChannel|g" \
        -e "s|<RejectChannel>|$RejectChannel|g" \
        -e "s|<Start>|$Start|g" \
        -e "s|<Help>|$Help|g" \
        -e "s|<DB_Generate>|$DB_Generate|g" \
        -e "s|<DB_UseMySQL>|$DB_UseMySQL|g" \
        -e "s|<DB_LogSQL>|$DB_LogSQL|g" \
        -e "s|<DB_DbHost>|$DB_DbHost|g" \
        -e "s|<DB_DbPort>|$DB_DbPort|g" \
        -e "s|<DB_DbName>|$DB_DbName|g" \
        -e "s|<DB_DbUser>|$DB_DbUser|g" \
        -e "s|<DB_DbPassword>|$DB_DbPassword|g" appsettings.json
fi

cat appsettings.json

./XinjingdailyBot.WebAPI