-- str = Console:ReadLine() -- считывание строки
require "scripts/settings"
require "scripts/protocol"

function Main()
	--local file = io.open("data/something.txt", "r")
	--Console:Log(file:read())
	--Console:Log(file:read())
	--file:close()
	Protocol:Connect("151.80.154.183", 29000)

	
	--file = io.open("test.lua", "a")
	--io.output(file)
	--io.write("-- End of the test.lua file")
	--io.close(file)
end

function ErrorInfo(code)
	if code == 0x03 then
		Console:Error("Неправильный логин или пароль")
	elseif code == 0x08 then
		Console:Error("Ошибка сервера")
	elseif code == 0x18 then
		-- Персонаж/аккаунт заблокирован?
	end
end

function AuthSuccess()
	Console:Success("Авторизация прошла успешно")
end

function RoleList_Re(roleid, gender, race, occupation, level, level2, name)
	Console:Log(string.format("%s - %s %s %s", #Roles, name, GetOccupation(occupation), level))
end

function GetOccupation(occupation)
	if occupation == 0 then
		return "Воин"
	elseif occupation == 1 then
		return "Маг"
	elseif occupation == 2 then
		return "Шаман"
	elseif occupation == 3 then
		return "Друид"
	elseif occupation == 4 then
		return "Оборотень"
	elseif occupation == 5 then
		return "Убийца"
	elseif occupation == 6 then
		return "Лучник"
	elseif occupation == 7 then
		return "Жрец"
	elseif occupation == 8 then
		return "Страж"
	elseif occupation == 9 then
		return "Мистик"
	else
		return "Неизвестно"
	end
end

function RoleList_End()
	Console:Print("Введите номер персонажа: ", false)
	while true do
		local index = tonumber(Console:ReadLine())
		if index ~= nil then
			if index < 1 or index > #Roles then
				Console:Print("Нет персонажа с таким индексом: ", false)
			else
				SelectRole(index)
				break
			end
		else
			Console:Print("Номер персонажа должен содержать только цифры: ", false)
		end
	end
end

--Console:Log("Приложение запущено")
--Console:Log(version)
--Console:Warning("Введенная строка: " .. str)
--Console:ReadLine() -- пауза