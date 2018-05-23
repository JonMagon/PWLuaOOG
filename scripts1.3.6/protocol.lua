require "scripts/binary"

Roles = {}
AlreadyGM = {}

function Connected()
	Console:Log("Соединение установлено")
end

function Disconnected()
	Console:Log("Соединение разорвано")
	Console:Log("Для выхода нажмите Enter")
	Console:ReadLine()
end

function Received(opcode, length)
	--Console:Log("Принят пакет от сервера: 0x" .. NumToHex(opcode))
	if opcode == 0x01 then
		LogginAnnounce()
	elseif opcode == 0x03 then
		EnchashKey = ReceivedPacket:ReadBytes(ReceivedPacket:ReadByte())
		AuthSuccess()
		CMKey()
	elseif opcode == 0x04 then
		AccountKey = ReceivedPacket:ReadDword()
		RoleList(-1)
	elseif opcode == 0x05 then
		ErrorInfo(ReceivedPacket:ReadByte())
	elseif opcode == 0x47 then
		EnterWorld()
	elseif opcode == 0x53 then
		ReceivedPacket:Seek(4)
		local nextslot = ReceivedPacket:ReadDword()
		ReceivedPacket:Seek(8)
		if ReceivedPacket:ReadByte() == 1 then -- isChar
			local roleid = ReceivedPacket:ReadDword()
			Roles[#Roles + 1] = roleid
			local gender = ReceivedPacket:ReadByte()
			local race = ReceivedPacket:ReadByte()
			local occupation = ReceivedPacket:ReadByte()
			local level = ReceivedPacket:ReadDword()
			local level2 = ReceivedPacket:ReadDword() -- Культивация/Статус
			local name = ReceivedPacket:ReadUString()
			RoleList_Re(roleid, gender, race, occupation, level, level2, name)
			RoleList(nextslot)
		else
			RoleList_End()
		end
	elseif opcode == 0x7B then
		ReceivedPacket:Seek(9)
		local timeleft = ReceivedPacket:ReadDword()
		local setbantime = ReceivedPacket:ReadDword()
		local message = ReceivedPacket:ReadUString()
		Console:Warning(string.format("Вход запрещен.\nПричина запрета: %s\nВремя, оставшееся до разблокировки: %s мин", message, math.floor(timeleft / 60)))
	elseif opcode == 0x50 then
		PublicChat()
	end
end

function ReceivedSubPacket(opcode, length)
	--Console:Log("Принят пакет " .. opcode)
	if opcode == 0x08 then
		Console:Success("Персонаж вышел в мир")
	elseif opcode == 0xDE then
		SendPacket:WriteDword(ReceivedPacket:ReadDword())
		SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
		SendPacket:PackContainer(0x61)
		SendPacket:Send(0x22)
		--PublicChat("{magic_code}<0><0:0>{magic_code}<0><0:0>{magic_code}<0><0:0>")
		--Console:Log("Получен пакет «залезь на руки»")
	end
end


function PrivateChat(wid, nick, message)
	SendPacket:WriteByte(0x0A)
	SendPacket:WriteByte(0xE8)
	SendPacket:WriteByte(0x00)
	SendPacket:WriteDword(RoleID, true)
	SendPacket:WriteUString(nick)
	SendPacket:WriteDword(wid, true)
	SendPacket:WriteUString(message)
	SendPacket:WriteByte(0x00)
	SendPacket:WriteDword(wid, true)
	SendPacket:Send(0x60)
	Console:Success(string.format("to %s: %s", nick, message))
end

function SelectRole(roleindex)
	RoleID = Roles[roleindex]
	SendPacket:WriteDword(RoleID, true)
	SendPacket:WriteByte(0)
	SendPacket:Send(0x46)
	--Console:Log("Отправлен пакет 0x46")
end

function EnterWorld()
	SendPacket:WriteDword(RoleID, true)
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:Send(0x48)
	--Console:Log("Отправлен пакет 0x48")
end

function RoleList(slot)
	SendPacket:WriteDword(AccountKey, true)
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:WriteDword(slot, true)
	SendPacket:Send(0x52)
	--Console:Log("Отправлен пакет 0x52")
end

function CMKey()
	Math:RandomTable("SMKey", 16)
	SendPacket:WriteByte(#SMKey)
	SendPacket:WriteBytes(SMKey)
	SendPacket:WriteByte(0x01) -- Force
	Protocol:InitRC4(EnchashKey, SMKey, hash, login)
	SendPacket:Send(0x03)
	--Console:Log("Отправлен пакет 0x02")
end

function LogginAnnounce()
	SendPacket:WriteString(login)
	Crypt:GetHash(login, password, ReceivedPacket:ReadBytes(ReceivedPacket:ReadByte()), "hash")
	SendPacket:WriteByte(#hash)
	SendPacket:WriteBytes(hash)
	SendPacket:WriteBytes({0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF}) -- Лень описывать структуру
	SendPacket:Send(0x02)
	--Console:Log("Отправлен пакет 0x03")
end