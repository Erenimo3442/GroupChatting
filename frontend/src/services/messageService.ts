import { Client, SendMessageDto } from "../generated/api-client";
import { fetch } from "../httpClient";
import type { FileParameter, MessageResponseDto } from "../generated/api-client";
import { API_BASE_URL } from "../config";

const messagesClient = new Client(API_BASE_URL, { fetch });

export const fetchMessages = async (groupId: string): Promise<MessageResponseDto[]> => {
  const items = await messagesClient.messagesAll(groupId, 1, 50, undefined);
  items.sort((a: MessageResponseDto, b: MessageResponseDto) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
  return items;
};

export const sendMessage = async (
  messageData: { groupId: string; content: string; fileUrl?: string; mimeType?: string }
): Promise<MessageResponseDto> => {
  const sendMessageDto = new SendMessageDto({
    content: messageData.content,
    fileUrl: messageData.fileUrl,
    mimeType: messageData.mimeType,
  });
  return messagesClient.messagesPOST(messageData.groupId, sendMessageDto);
};

export const uploadFile = async (
  fileData: { groupId: string; file: FileParameter, content?: string }
): Promise<MessageResponseDto> => {
  return messagesClient.upload(fileData.groupId, fileData.file, fileData.content);
}
