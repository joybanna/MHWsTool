import fs from "node:fs";
import path from "node:path";
import sharp from "../third_party/mhwilds-calculator/node_modules/sharp/lib/index.js";

const root = path.resolve(import.meta.dirname, "..");
const output = path.join(root, "MHWsTool", "Assets", "results");
fs.mkdirSync(output, { recursive: true });

for (const [kind, file] of Object.entries({
  head: "game8-head.webp",
  chest: "game8-chest.webp",
  arms: "game8-arms.webp",
  waist: "game8-waist.webp",
  legs: "game8-legs.webp",
  charm: "game8-charm.webp",
  "slot-category-armor": "game8-slot-category-armor.webp",
  "slot-category-weapon": "game8-slot-category-weapon.webp",
})) {
  await sharp(path.join(import.meta.dirname, file))
    .resize(48, 48)
    .png()
    .toFile(path.join(output, `${kind}.png`));
}

for (const level of [1, 2, 3, 4]) {
  const source = path.join(import.meta.dirname, `game8-slot-${level}.png`);
  const { data, info } = await sharp(source).resize(40, 40).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  for (let offset = 0; offset < data.length; offset += 4) {
    data[offset] = 230;
    data[offset + 1] = 236;
    data[offset + 2] = 242;
  }
  await sharp(data, { raw: info }).png().toFile(path.join(output, `slot-${level}.png`));
  if (level <= 3) {
    await sharp(data, { raw: info }).png().toFile(path.join(output, `decoration-${level}.png`));
  }
}

for (const name of ["fire", "water", "thunder", "ice", "dragon"]) {
  await sharp(path.join(import.meta.dirname, `game8-element-${name}.png`))
    .resize(40, 40)
    .png()
    .toFile(path.join(output, `element-${name}.png`));
}
