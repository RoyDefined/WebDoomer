export const gameTypeArray = [
    'cooperative',
    'survival',
    'invasion',
    'deathmatch',
    'teamPlay',
    'duel',
    'terminator',
    'lastManStanding',
    'teamLms',
    'possession',
    'teamPossession',
    'teamgame',
    'ctf',
    'oneFlagCtf',
    'skulltag',
    'domination',
] as const;
type GameTypeTuple = typeof gameTypeArray;
export type GameType = GameTypeTuple[number];
