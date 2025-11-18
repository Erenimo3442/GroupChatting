import { Client, CreateGroupDto } from "../generated/api-client";
import { fetch } from "../httpClient";
import type { GroupResponseDto } from "../generated/api-client";

const groupsClient = new Client("http://localhost:8080", { fetch });

export const getGroups = async (): Promise<GroupResponseDto[]> => {
    return groupsClient.public();
};

export const createGroup = async (groupData: { name: string; isPublic?: boolean; }) => {
    const createGroupDto = new CreateGroupDto({
        name: groupData.name,
        isPublic: groupData.isPublic ?? true
    });
    return groupsClient.groups(createGroupDto);
};
