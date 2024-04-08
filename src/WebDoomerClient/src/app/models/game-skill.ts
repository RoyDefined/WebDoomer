export const gameSkillArray = ['easiest', 'easy', 'average', 'hard', 'hardest', 'custom'] as const;
type GameSkillTuple = typeof gameSkillArray;
export type GameSkill = GameSkillTuple[number];
