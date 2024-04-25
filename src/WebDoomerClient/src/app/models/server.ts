import { z } from 'zod';
import Sqids from 'sqids';
import { ServerFetchState } from './server-fetch-state';
import { serverSchema } from './server-schema';
import { serverListSchema } from '../services/api/server-list-schema';
import { serverIdListSchema } from '../services/api/server-id-list-schema';
import { serverDetailedSchema } from '../services/api/server-detailed-schema';

/**
 * Represents an available server.
 */
export class Server implements z.infer<typeof serverSchema> {
    // Internal data.
    public state: ServerFetchState;
    public fetching: boolean;

    // Decoded from the id. Not actually parsed but defined like this for convenience.
    public ip: z.infer<typeof serverSchema.shape.ip>;
    public port: z.infer<typeof serverSchema.shape.port>;

    // Actual received data.
    public id: z.infer<typeof serverSchema.shape.id>;
    public engine: z.infer<typeof serverSchema.shape.engine>;
    public playingClientCount: z.infer<typeof serverSchema.shape.playingClientCount>;
    public spectatingClientCount: z.infer<typeof serverSchema.shape.spectatingClientCount>;
    public botCount: z.infer<typeof serverSchema.shape.botCount>;
    public name: z.infer<typeof serverSchema.shape.name>;
    public url: z.infer<typeof serverSchema.shape.url>;
    public email: z.infer<typeof serverSchema.shape.email>;
    public mapName: z.infer<typeof serverSchema.shape.mapName>;
    public maxClients: z.infer<typeof serverSchema.shape.maxClients>;
    public maxPlayers: z.infer<typeof serverSchema.shape.maxPlayers>;
    public pwadCollection: z.infer<typeof serverSchema.shape.pwadCollection>;
    public gameType: z.infer<typeof serverSchema.shape.gameType>;
    public gameTypeInstagib: z.infer<typeof serverSchema.shape.gameTypeInstagib>;
    public gameTypeBuckshot: z.infer<typeof serverSchema.shape.gameTypeBuckshot>;
    public gameNameType: z.infer<typeof serverSchema.shape.gameNameType>;
    public iwad: z.infer<typeof serverSchema.shape.iwad>;
    public forcePassword: z.infer<typeof serverSchema.shape.forcePassword>;
    public forceJoinPassword: z.infer<typeof serverSchema.shape.forceJoinPassword>;
    public gameSkillType: z.infer<typeof serverSchema.shape.gameSkillType>;
    public botSkillType: z.infer<typeof serverSchema.shape.botSkillType>;
    public fragLimit: z.infer<typeof serverSchema.shape.fragLimit>;
    public timeLimit: z.infer<typeof serverSchema.shape.timeLimit>;
    public timeLeft: z.infer<typeof serverSchema.shape.timeLeft>;
    public duelLimit: z.infer<typeof serverSchema.shape.duelLimit>;
    public pointLimit: z.infer<typeof serverSchema.shape.pointLimit>;
    public winLimit: z.infer<typeof serverSchema.shape.winLimit>;
    public teamDamage: z.infer<typeof serverSchema.shape.teamDamage>;
    public numPlayers: z.infer<typeof serverSchema.shape.numPlayers>;
    public playerDataCollection: z.infer<typeof serverSchema.shape.playerDataCollection>;
    public teamCount: z.infer<typeof serverSchema.shape.teamCount>;
    public teamInfoCollection: z.infer<typeof serverSchema.shape.teamInfoCollection>;
    public testingServerName: z.infer<typeof serverSchema.shape.testingServerName>;
    public dmFlagCollection: z.infer<typeof serverSchema.shape.dmFlagCollection>;
    public securitySettings: z.infer<typeof serverSchema.shape.securitySettings>;
    public dehackedNameCollection: z.infer<typeof serverSchema.shape.dehackedNameCollection>;
    public country: z.infer<typeof serverSchema.shape.country>;
    public gameModeName: z.infer<typeof serverSchema.shape.gameModeName>;
    public gameModeShortName: z.infer<typeof serverSchema.shape.gameModeShortName>;

    constructor(sqidsConverter: Sqids, data: z.infer<typeof serverIdListSchema>) {
        const decoded = sqidsConverter.decode(data);
        this.ip = this.intToIP(decoded[0]);
        this.port = decoded[1];
        this.id = data;

        this.state = 'id';
        this.fetching = false;
    }

    public patchAsList(data: z.infer<typeof serverListSchema>) {
        // TODO: Verify id.

        this.engine = data.engine;
        this.playingClientCount = data.playingClientCount;
        this.spectatingClientCount = data.spectatingClientCount;
        this.botCount = data.botCount;
        this.name = data.name;
        this.maxClients = data.maxClients;
        this.forcePassword = data.forcePassword;
        this.forceJoinPassword = data.forceJoinPassword;
        this.country = data.country;

        this.state = 'list';
    }

    public patchAsDetailed(data: z.infer<typeof serverDetailedSchema>) {
        // TODO: Verify id.

        this.engine = data.engine;
        this.name = data.name;
        this.url = data.url;
        this.email = data.email;
        this.mapName = data.mapName;
        this.maxClients = data.maxClients;
        this.maxPlayers = data.maxPlayers;
        this.pwadCollection = data.pwadCollection;
        this.gameType = data.gameType;
        this.gameTypeInstagib = data.gameTypeInstagib;
        this.gameTypeBuckshot = data.gameTypeBuckshot;
        this.gameNameType = data.gameNameType;
        this.iwad = data.iwad;
        this.forcePassword = data.forcePassword;
        this.forceJoinPassword = data.forceJoinPassword;
        this.gameSkillType = data.gameSkillType;
        this.botSkillType = data.botSkillType;
        this.fragLimit = data.fragLimit;
        this.timeLimit = data.timeLimit;
        this.timeLeft = data.timeLeft;
        this.duelLimit = data.duelLimit;
        this.pointLimit = data.pointLimit;
        this.winLimit = data.winLimit;
        this.teamDamage = data.teamDamage;
        this.numPlayers = data.numPlayers;
        this.playerDataCollection = data.playerDataCollection;
        this.teamCount = data.teamCount;
        this.teamInfoCollection = data.teamInfoCollection;
        this.testingServerName = data.testingServerName;
        this.dmFlagCollection = data.dmFlagCollection;
        this.securitySettings = data.securitySettings;
        this.dehackedNameCollection = data.dehackedNameCollection;
        this.country = data.country;
        this.gameModeName = data.gameModeName;
        this.gameModeShortName = data.gameModeShortName;

        this.state = 'detailed';
    }

    private intToIP(input: number) {
        var part1 = input & 255;
        var part2 = (input >> 8) & 255;
        var part3 = (input >> 16) & 255;
        var part4 = (input >> 24) & 255;

        return part4 + '.' + part3 + '.' + part2 + '.' + part1;
    }
}
