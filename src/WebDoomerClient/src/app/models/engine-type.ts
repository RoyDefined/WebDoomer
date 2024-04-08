export const engineTypeArray = ['QZandronum', 'Zandronum'] as const;
type EngineTypeTuple = typeof engineTypeArray;
export type EngineType = EngineTypeTuple[number];
