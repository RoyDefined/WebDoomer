export const botSkillArray = ['easiest', 'easy', 'average', 'hard', 'hardest', 'custom'] as const;
type BotSkillTuple = typeof botSkillArray;
export type BotSkill = BotSkillTuple[number];
