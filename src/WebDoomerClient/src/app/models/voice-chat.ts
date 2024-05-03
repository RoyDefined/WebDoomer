export const voiceChatArray = ['none', 'everyone', 'teammates', 'separately'] as const;
type VoiceChatTuple = typeof voiceChatArray;
export type VoiceChat = VoiceChatTuple[number];
