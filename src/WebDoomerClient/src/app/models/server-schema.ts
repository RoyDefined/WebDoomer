import { z } from 'zod';
import { engineTypeArray } from './engine-type';
import { pwadSchema } from './pwad-schema';
import { gameTypeArray } from './game-type';
import { gameNameArray } from './game-name';
import { gameSkillArray } from './game-skill';
import { botSkillArray } from './bot-skill';
import { playerSchema } from './player-schema';
import { teamSchema } from './team-schema';
import { dmFlagSchema } from './dm-flag-schema';
import { dehackedNameSchema } from './dehacked-name-schema';
import { voiceChatArray } from './voice-chat';

/**
 * Represents the base schema used to define a server.
 */
export const serverSchema = z
    .object({
        id: z.string().min(1),
        ip: z.string().min(1),
        port: z.number().nonnegative(),
        engine: z.number().nonnegative().lte(engineTypeArray.length).optional(),
        playingClientCount: z.number().nonnegative().nullable().optional(),
        spectatingClientCount: z.number().nonnegative().nullable().optional(),
        botCount: z.number().nonnegative().nullable().optional(),
        name: z
            .string()
            .min(1)
            .nullable()
            .optional()
            .catch(() => null),
        url: z
            .string()
            .min(1)
            .nullable()
            .optional()
            .catch(() => null),
        email: z
            .string()
            .min(1)
            .nullable()
            .optional()
            .catch(() => null),
        mapName: z
            .string()
            .min(1)
            .nullable()
            .optional()
            .catch(() => null),
        maxClients: z.number().nonnegative().nullable().optional(),
        maxPlayers: z.number().nonnegative().nullable().optional(),
        pwadCollection: pwadSchema.array().nullable().optional(),
        gameType: z.number().nonnegative().lte(gameTypeArray.length).nullable().optional(),
        gameTypeInstagib: z.boolean().nullable().optional(),
        gameTypeBuckshot: z.boolean().nullable().optional(),
        gameNameType: z.number().nonnegative().lte(gameNameArray.length).nullable().optional(),
        iwad: z
            .string()
            .min(1)
            .nullable()
            .optional()
            .catch(() => null),
        forcePassword: z.boolean().nullable().optional(),
        forceJoinPassword: z.boolean().nullable().optional(),
        gameSkillType: z.number().nonnegative().lte(gameSkillArray.length).nullable().optional(),
        botSkillType: z.number().nonnegative().lte(botSkillArray.length).nullable().optional(),
        fragLimit: z.number().nonnegative().nullable().optional(),
        timeLimit: z.number().nonnegative().nullable().optional(),
        timeLeft: z.number().nonnegative().nullable().optional(),
        duelLimit: z.number().nonnegative().nullable().optional(),
        pointLimit: z.number().nonnegative().nullable().optional(),
        winLimit: z.number().nonnegative().nullable().optional(),
        teamDamage: z.number().nonnegative().nullable().optional(),
        numPlayers: z.number().nonnegative().nullable().optional(),
        playerDataCollection: playerSchema.array().nullable().optional(),
        teamCount: z.number().nonnegative().nullable().optional(),
        teamInfoCollection: teamSchema.array().nullable().optional(),
        testingServerName: z.string().min(1).nullable().optional(),
        dmFlagCollection: dmFlagSchema.array().nullable().optional(),
        securitySettings: z.boolean().nullable().optional(),
        dehackedNameCollection: dehackedNameSchema.array().nullable().optional(),
        country: z
            .string()
            .min(1)
            .nullable()
            .optional()
            .catch(() => null),
        gameModeName: z
            .string()
            .min(1)
            .nullable()
            .optional()
            .catch(() => null),
        gameModeShortName: z
            .string()
            .min(1)
            .nullable()
            .optional()
            .catch(() => null),
        voiceChatType: z
            .number()
            .nonnegative()
            .lte(voiceChatArray.length)
            .nullable()
            .optional()
            .catch(() => 0),
    })
    .strict();
