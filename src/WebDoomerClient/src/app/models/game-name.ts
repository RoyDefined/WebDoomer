export const gameNameArray = ['unknown', 'doom', 'doom2', 'heretic', 'hexen', 'error'] as const;
type GameNameTuple = typeof gameNameArray;
export type GameName = GameNameTuple[number];
