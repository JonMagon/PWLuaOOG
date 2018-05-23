-- str = Console:ReadLine() -- ���������� ������
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
		Console:Error("������������ ����� ��� ������")
	elseif code == 0x08 then
		Console:Error("������ �������")
	elseif code == 0x18 then
		-- ��������/������� ������������?
	end
end

function AuthSuccess()
	Console:Success("����������� ������ �������")
end

function RoleList_Re(roleid, gender, race, occupation, level, level2, name)
	Console:Log(string.format("%s - %s %s %s", #Roles, name, GetOccupation(occupation), level))
end

function GetOccupation(occupation)
	if occupation == 0 then
		return "����"
	elseif occupation == 1 then
		return "���"
	elseif occupation == 2 then
		return "�����"
	elseif occupation == 3 then
		return "�����"
	elseif occupation == 4 then
		return "���������"
	elseif occupation == 5 then
		return "������"
	elseif occupation == 6 then
		return "������"
	elseif occupation == 7 then
		return "����"
	elseif occupation == 8 then
		return "�����"
	elseif occupation == 9 then
		return "������"
	else
		return "����������"
	end
end

function RoleList_End()
	Console:Print("������� ����� ���������: ", false)
	while true do
		local index = tonumber(Console:ReadLine())
		if index ~= nil then
			if index < 1 or index > #Roles then
				Console:Print("��� ��������� � ����� ��������: ", false)
			else
				SelectRole(index)
				break
			end
		else
			Console:Print("����� ��������� ������ ��������� ������ �����: ", false)
		end
	end
end

--Console:Log("���������� ��������")
--Console:Log(version)
--Console:Warning("��������� ������: " .. str)
--Console:ReadLine() -- �����