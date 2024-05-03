import { z } from 'zod';
import { engineTypeArray } from '../../models/engine-type';
import { voiceChatArray } from '../../models/voice-chat';

/**
 * Represents the fetch result of a single entry in a list of servers.
 */
export const serverListSchema = z
    .object({
        id: z.string().min(1),
        engine: z.number().nonnegative().lte(engineTypeArray.length),
        name: z.string().min(1).nullable(),
        playingClientCount: z.number().nonnegative().nullable(),
        spectatingClientCount: z.number().nonnegative().nullable(),
        botCount: z.number().nonnegative().nullable(),
        maxClients: z.number().nonnegative().nullable(),
        forcePassword: z.boolean().nullable(),
        forceJoinPassword: z.boolean().nullable(),
        country: z.string().min(1).nullable(),
        voiceChatType: z.number().nonnegative().lte(voiceChatArray.length).nullable(),
    })
    .strict();

export const serverListArraySchema = serverListSchema.array();
