import { z } from 'zod';
import { engineTypeArray } from '../../models/engine-type';
import { pwadSchema } from '../../models/pwad-schema';
import { gameTypeArray } from '../../models/game-type';
import { gameNameArray } from '../../models/game-name';
import { gameSkillArray } from '../../models/game-skill';
import { botSkillArray } from '../../models/bot-skill';
import { playerSchema } from '../../models/player-schema';
import { teamSchema } from '../../models/team-schema';
import { dmFlagSchema } from '../../models/dm-flag-schema';
import { dehackedNameSchema } from '../../models/dehacked-name-schema';

/**
 * Represents the fetch result of detailed server.
 */
export const serverDetailedSchema = z
    .object({
        id: z.string().min(1),
        engine: z.number().nonnegative().lte(engineTypeArray.length),
        playingClientCount: z.number().nonnegative().nullable(),
        spectatingClientCount: z.number().nonnegative().nullable(),
        botCount: z.number().nonnegative().nullable(),
        name: z.string().min(1).nullable(),
        url: z.string().min(1).nullable(),
        email: z.string().min(1).nullable(),
        mapName: z.string().min(1).nullable(),
        maxClients: z.number().nonnegative().nullable(),
        maxPlayers: z.number().nonnegative().nullable(),
        pwadCollection: pwadSchema.array().nullable(),
        gameType: z.number().nonnegative().lte(gameTypeArray.length).nullable(),
        gameTypeInstagib: z.boolean().nullable(),
        gameTypeBuckshot: z.boolean().nullable(),
        gameNameType: z.number().nonnegative().lte(gameNameArray.length).nullable(),
        iwad: z.string().min(1).nullable(),
        forcePassword: z.boolean().nullable(),
        forceJoinPassword: z.boolean().nullable(),
        gameSkillType: z.number().nonnegative().lte(gameSkillArray.length).nullable(),
        botSkillType: z.number().nonnegative().lte(botSkillArray.length).nullable(),
        fragLimit: z.number().nonnegative().nullable(),
        timeLimit: z.number().nonnegative().nullable(),
        timeLeft: z.number().nonnegative().nullable(),
        duelLimit: z.number().nonnegative().nullable(),
        pointLimit: z.number().nonnegative().nullable(),
        winLimit: z.number().nonnegative().nullable(),
        teamDamage: z.number().nonnegative().nullable(),
        numPlayers: z.number().nonnegative().nullable(),
        playerDataCollection: playerSchema.array().nullable(),
        teamCount: z.number().nonnegative().nullable(),
        teamInfoCollection: teamSchema.array().nullable(),
        testingServerName: z.string().min(1).nullable(),
        dmFlagCollection: dmFlagSchema.array().nullable(),
        securitySettings: z.boolean().nullable(),
        dehackedNameCollection: dehackedNameSchema.array().nullable(),
        country: z.string().min(1).nullable(),
        gameModeName: z.string().min(1).nullable(),
        gameModeShortName: z.string().min(1).nullable(),
    })
    .strict();
