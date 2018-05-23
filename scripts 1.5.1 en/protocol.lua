Roles = {}

function Connected()
	Console:Log("Successful connection")
end

function Disconnected()
	Console:Log("This connection has been closed")
	Console:Log("Press enter to exit...")
	Console:ReadLine()
end

function Received(opcode, length)
	if opcode == 0x01 then
		LogginAnnounce()
	elseif opcode == 0x02 then
		EnchashKey = ReceivedPacket:ReadBytes(ReceivedPacket:ReadByte())
		AuthSuccess()
		CMKey()
	elseif opcode == 0x04 then
		AccountKey = ReceivedPacket:ReadDword()
		RoleList(-1)
	elseif opcode == 0x05 then
		ErrorInfo(ReceivedPacket:ReadDword())
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
			local level2 = ReceivedPacket:ReadDword() -- Cultivation
			local name = ReceivedPacket:ReadUString()
			RoleList_Re(roleid, gender, race, occupation, level, level2, name)
			RoleList(nextslot)
		else
			RoleList_End()
		end
	elseif opcode == 0x85 then
		ReceivedPacket:ReadByte() -- Type
		ReceivedPacket:ReadByte() -- Emotion
		local wid = ReceivedPacket:ReadDword()
		local nick = ReceivedPacket:ReadUString()
		local message = ReceivedPacket:ReadUString()
		if wid ~= 100 then -- FromWID
			Console:Log(string.format("from %s: %s", nick, message))
		end
	elseif opcode == 0x7B then
		ReceivedPacket:Seek(9)
		local timeleft = ReceivedPacket:ReadDword()
		local setbantime = ReceivedPacket:ReadDword()
		local message = ReceivedPacket:ReadUString()
		Console:Warning(string.format("Log in banned\nBanned: %s\nBanned for %s more minutes", message, math.floor(timeleft / 60)))
	end
end

function ReceivedSubPacket(opcode, length)
	if opcode == 0x0E then
		Console:Success("Entered the game world")
	elseif opcode == 0xDE then
		SendPacket:WriteDword(ReceivedPacket:ReadDword())
		SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
		SendPacket:PackContainer(0x61)
		SendPacket:Send(0x22)
	end
end

function SelectRole(roleindex)
	RoleID = Roles[roleindex]
	SendPacket:WriteDword(RoleID, true)
	SendPacket:WriteByte(0)
	SendPacket:Send(0x46)
end

function EnterWorld()
	SendPacket:WriteDword(RoleID, true)
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:Send(0x48)
end

function RoleList(slot)
	SendPacket:WriteDword(AccountKey, true)
	SendPacket:WriteBytes({0x00, 0x00, 0x00, 0x00})
	SendPacket:WriteDword(slot, true)
	SendPacket:Send(0x52)
end

function CMKey()
	Math:RandomTable("SMKey", 16)
	SendPacket:WriteByte(#SMKey)
	SendPacket:WriteBytes(SMKey)
	SendPacket:WriteByte(0x01)
	Protocol:InitRC4(EnchashKey, SMKey, hash, login)
	SendPacket:Send(0x02)
end

function LogginAnnounce()
	SendPacket:WriteString(login)
	Crypt:GetHash(login, password, ReceivedPacket:ReadBytes(ReceivedPacket:ReadByte()), "hash")
	SendPacket:WriteByte(#hash)
	SendPacket:WriteBytes(hash)
	SendPacket:WriteBytes({0x00, 0x04, 0xFF, 0xFF, 0xFF, 0xFF})
	SendPacket:Send(0x03)
end