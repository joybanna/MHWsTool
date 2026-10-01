import { expect, test } from "vitest";
import Attacks from "@/data/attacks";
import { WeaponTypes } from "@/data";
import { Monsters } from "@/data/monsters";
import { WeaponSkills } from "@/data/skills";
import { calculateAffinity, calculateAttack, calculateAverage, calculateCrit, calculateHit } from "@/model";
import { Target, Weapon } from "@/types";

const target: Target = { wound: false, Slash: 80, Blunt: 80, Shot: 80, Fire: 30, Water: 30, Thunder: 30, Ice: 30, Dragon: 30 };
const weapon = { name: "Custom Weapon", type: "Sword and Shield", attack: 200, affinity: 0,
  sharpness: [0, 0, 0, 0, 0, 150, 0], handicraft: [0, 0, 0, 0], slots: [0, 0, 0], skills: {} } as Weapon;

test("reference damage uses motion value, sharpness, hitzone, crit and affinity", () => {
  const attack = Attacks["Sword and Shield"].find(a => a.name === "Advancing Slash")!;
  const buffs = { Powercharm: { attack: 6 } };
  const hit = calculateHit(weapon, buffs, attack, target);
  const crit = calculateCrit(weapon, buffs, attack, target, 1.25, 1);
  expect(hit).toBe(47.9);
  expect(crit).toBe(59.8);
  expect(calculateAverage(hit, crit, 50)).toBe(53.85);
  expect(calculateAverage(100, 75, -50)).toBe(87.5);
});

test("buffs and element independently increase damage", () => {
  const attack = Attacks["Sword and Shield"][0];
  const buffed = calculateAttack(200, { boost: { attackMul: 1.1, attack: 10 }, charm: { attack: 6 } });
  expect(buffed).toBe(236);
  const raw = calculateHit(weapon, {}, attack, target);
  const elemental = calculateHit({ ...weapon, element: { type: "Fire", value: 300 } } as Weapon, {}, attack, target);
  expect(elemental).toBeGreaterThan(raw);
  expect(calculateHit(weapon, {}, attack, { ...target, Slash: 40 })).toBeLessThan(raw);
});

test("affinity caps and wounded weak spots use current skill data", () => {
  expect(calculateAffinity({ affinity: 120, target, rawType: "Slash" })).toBe(100);
  expect(calculateAffinity({ affinity: -120, target, rawType: "Slash" })).toBe(-100);
  const buffs = { weakness: { weakness: { affinity: 20 }, wound: { affinity: 10 } } };
  expect(calculateAffinity({ affinity: 0, buffs, target: { ...target, wound: true }, rawType: "Slash" })).toBe(30);
  expect(calculateAffinity({ affinity: 0, buffs, target: { ...target, Slash: 20 }, rawType: "Slash" })).toBe(0);
});

test.each(WeaponTypes)("%s has offline attacks with finite damage", type => {
  expect(Attacks[type].length).toBeGreaterThan(0);
  const w = { ...weapon, type, sharpness: type.includes("Bow") ? undefined : weapon.sharpness } as Weapon;
  for (const attack of Attacks[type]) {
    expect(Number.isFinite(calculateHit(w, {}, attack, target)), attack.name).toBe(true);
  }
});

test("current Attack Boost level 5 applies its multiplier and flat bonus", () => {
  const levels = (WeaponSkills["Attack Boost"] as { levels: Record<number, object> }).levels;
  expect(calculateAttack(200, { "Attack Boost": levels[5] })).toBe(217);
});

test("monster hitzones are bundled and within valid range", () => {
  expect(Object.keys(Monsters).length).toBeGreaterThan(20);
  for (const parts of Object.values(Monsters)) {
    for (const part of Object.values(parts)) {
      for (const [key, value] of Object.entries(part)) {
        if (key !== "wound") expect(value).toBeGreaterThanOrEqual(0);
        if (key !== "wound") expect(value).toBeLessThanOrEqual(100);
      }
    }
  }
});
