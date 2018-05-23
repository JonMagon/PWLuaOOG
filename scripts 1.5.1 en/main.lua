require "scripts/settings"
require "scripts/protocol"

function Main()
	Protocol:Connect(server, port)
end

function ErrorInfo(code)
	if code == 0x03 then
		Console:Error("Incorrect account name or password")
	elseif code == 0x08 then
		Console:Error("Server network error")
	elseif code == 0x20 then
		Console:Error("You have been booted")
	end
end

function AuthSuccess()
	Console:Success("Successful authorization")
end

function RoleList_Re(roleid, gender, race, occupation, level, level2, name)
	Console:Log(string.format("%s - %s %s %s", #Roles, name, GetOccupation(occupation), level))
end

function GetOccupation(occupation)
	if occupation == 0 then
		return "Blademaster"
	elseif occupation == 1 then
		return "Wizard"
	elseif occupation == 2 then
		return "Psychic"
	elseif occupation == 3 then
		return "Venomancer"
	elseif occupation == 4 then
		return "Barbarian"
	elseif occupation == 5 then
		return "Assassin"
	elseif occupation == 6 then
		return "Archer"
	elseif occupation == 7 then
		return "Cleric"
	elseif occupation == 8 then
		return "Seeker"
	elseif occupation == 9 then
		return "Mystic"
	else
		return "Unknown"
	end
end

function RoleList_End()
	Console:Print("Type index: ", false)
	while true do
		local index = tonumber(Console:ReadLine())
		if index ~= nil then
			if index < 1 or index > #Roles then
				Console:Print(string.format("You must type index between 1 and %s: ", #Roles), false)
			else
				SelectRole(index)
				break
			end
		else
			Console:Print("Invalid index: ", false)
		end
	end
end