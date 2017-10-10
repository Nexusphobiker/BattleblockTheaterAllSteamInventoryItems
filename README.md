# Easy proof of concept of abusing BattleBlock Theaters steam market api.

//can remove a few lines if you just call the steamdll function directly
mov ebx,[BattleBlockTheater.exe+30E7C4] { [077CF960] }
mov ecx,[ebx+00000D50]
mov eax,[ecx]
mov edx,00000065 //string length
push edx
mov edx,01520036 //string start address
push edx
push 7FFF0001
call dword ptr [eax]
ret

string content example (3 diamonds) => {.."requestid":5,.."fields":["itemid","defindex","quantity"],.."generate":[50000,50000,50000].} 
Ids start at 
50000 for diamonds
50001 for yarn
50002-50321 (C352 - C491) for masks
etc.

.. = 0A 09 (new line, tab)
. = 0A (new line)

(all named functions are easy to find because of exception strings)
Functions to look at are RequestGenerateItems (gets called at level end when the diamonds of the level are added to your steam inventory)
Functions to look at are RequestExchangeItems (gets called by the randomizer functions for weapons and masks, beautiful to reverse)

lots of fun. steam plz no ban ty

![alt text](https://i.imgur.com/8LUGHKu.png)
