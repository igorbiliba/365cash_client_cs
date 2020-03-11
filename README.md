365cach client
=====================
- Аккаунты из accounts работают по очереди, если все аккаунты заняты, клиент будет ожидать освобождения аккаунта, время жизни задано в avg_time_create_sec.
- Чтобы очистить забаненные прокси- запустить программу два раза кликнув на нее, либо через консоль Lisa.exe --checkallproxy

Релизы
=====================
https://github.com/igorbiliba/host_exchage_cs/releases/

### Зависимости
- https://github.com/igorbiliba/host_exchage_cs
- https://github.com/igorbiliba/netex_client_cs
- https://github.com/igorbiliba/365cash_client_cs
- https://github.com/igorbiliba/mine_exchange_cs

Init
=====================
### 1 Создать файл (Settings.json) рядом с .exe
```js
{
  "accounts": [
	{
		"login":"softmaster",
		"passwrd":"12345Qwert"
	},
	{
		"login":"bituk",
		"passwrd":"12345Qwert"
	},
	{
		"login":"qiwicash",
		"passwrd":"12345Qwert"
	}
  ],
  "avg_time_create_sec": 18,
  "delay_after_login_from":3000,
  "delay_after_login_to":4000,
  "delay_before_each_step_from":100,
  "delay_before_each_step_to":100,
  "delay_before_get_qiwi_number_from":3000,
  "delay_before_get_qiwi_number_to":3000,
  "expireMinOneIp":16,
  "maxHoursTestPeriodProxy": 24
}
```
- delay_* - задержка для действий, чтобы эмулировать поведение пользователя
- expireMinOneIp - не юзать n минут блокированный ип
### 2 Создать файл (ProxySettings.json) рядом с .exe
```js
[
	{ "host":"amsterdam1.perfect-privacy.com",  "ip":"85.17.28.145",    "username": "bankubeda7", "password": "*" },
	{ "host":"amsterdam2.perfect-privacy.com",  "ip":"95.211.95.232",   "username": "bankubeda7", "password": "*" },
	{ "host":"amsterdam3.perfect-privacy.com",  "ip":"95.211.95.244",   "username": "bankubeda7", "password": "*" },
	{ "host":"amsterdam4.perfect-privacy.com",  "ip":"37.48.94.1",      "username": "bankubeda7", "password": "*" },
	{ "host":"amsterdam5.perfect-privacy.com",  "ip":"85.17.64.131",    "username": "bankubeda7", "password": "*" },
	{ "host":"basel1.perfect-privacy.com",      "ip":"82.199.134.162",  "username": "bankubeda7", "password": "*" },
	............
]
```
##### *Как парсить сайт с прокси, см ниже
- ProxyLog.json - лог использования прокси. Регистрирует удачны и неудачные транзы через прокси, если разница между уданой и неудачной, либо от любой, до неудачной больше maxHoursTestPeriodProxy- прокси считается мертвым и больше не используется.
### БАН
- Проверить или на клиенте бан ip - дважды нажав на него.
- Ип из-за частоты запросов, или когда ведет себя не как юзер может словить бан на 15 мин, тогда работет через прокси.
##### *Как парсить сайт с прокси, см ниже
API
=====================
### Создание
Lisa.exe --create 6000 +79060671232 3F12oBFJ72SdjZ5rAcVds1uRQTtgYLLFXz
#### ответ:
```js
{
  account : "+79060671232",
  comment : "#3877525#",
  btc_amount : 0.34564
}
```
### Курс
Lisa.exe --rate
#### ответ:
```js
{
  "rate":698445.7366,
  "balance":2.68
}
```
### Очистить прокси
Lisa.exe --checkallproxy
#### ответ:
```js
134
```
- количество проксей, что имею бан навсегда

Proxy
=====================
### Парсить https://www.perfect-privacy.com/en/customer/download/proxy-http:
#### Вставить в косоль:
```js
window.all = "";
$(".servergroup .col-8").each(function(key, el) {
	window.all += el.innerHTML + "\n----------";
})
window.list = window
	.all
	.replace(/<strong>Hostname:<\/strong>/gi, "")
	.replace(/<strong>IPv4:<\/strong>/gi, "")
	.replace(/<strong>Ports:<\/strong>/gi, "")
	.replace(/ /gi, "")
	.replace(/<br>/gi, "")
	.split("\n")
	.filter(
		el => el.indexOf("<strong>") === -1 && el.length > 0
	);
window.groups = window.list.join("\n").split("----------");
window.itemsInGroups = [];
for(var group of window.groups) {
	window.itemsInGroups.push(
		group
		.split("\n")
		.filter( el => el.length > 0 )
	);
}
var result = [];
for(var item of window.itemsInGroups) {
	if(typeof item[1] === "undefined") continue;
	if(item[1].indexOf(".") !== -1)
		result.push({
			host: item[0],
			ip: item[1]
		});
}
$("body").html(JSON.stringify(result));
```
