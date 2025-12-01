import { Client, CreateGroupDto } from "../generated/api-client";
import { fetch } from "../httpClient";
import type { GroupResponseDto } from "../generated/api-client";
import { API_BASE_URL } from "../config";

const groupsClient = new Client(API_BASE_URL, { fetch });

export const getGroups = async (): Promise<GroupResponseDto[]> => {
    return groupsClient.public();
};

export const createGroup = async (groupData: { name: string; isPublic?: boolean; }): Promise<GroupResponseDto> => {
    const createGroupDto = new CreateGroupDto({
        name: groupData.name,
        isPublic: groupData.isPublic ?? true
    });
    return groupsClient.groups(createGroupDto);
};
