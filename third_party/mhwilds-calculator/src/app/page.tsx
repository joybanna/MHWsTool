"use client";

// import { useState } from "react";
import { useEffect, useState } from "react";
import {
  AttacksCard,
  BuffsCard,
  EquipmentCard,
  // Notice,
  SkillPointCard,
  StatsCard,
  // Tab,
} from "@/components";
import { useBuild } from "@/store/builder";

export default function Builder() {
  const { reset } = useBuild();

  const [hasReset, setHasReset] = useState(false);

  useEffect(() => {
    if (!hasReset) {
      reset();
      setHasReset(true);
    }
  }, [reset, hasReset]);

  return (
    <div className="max-w-9xl mx-auto flex h-screen w-full flex-col gap-2 p-4">
      <div className="flex flex-col gap-2 xl:flex-row">
        <div className="flex flex-5 flex-col gap-2">
          {/* <ManualWeaponCard />
          <ManualSkillsCard /> */}
          <EquipmentCard />
          <BuffsCard />
        </div>
        <div className="flex flex-3 flex-col gap-2">
          <StatsCard />
          <SkillPointCard />
          {/* <Card>
            <h1>Debug</h1>
            <textarea
              className="font-mono text-xs"
              value={JSON.stringify({ customCharmSkills }, undefined, 2)}
              rows={30}
              readOnly
            />
          </Card> */}
        </div>
        <div className="flex-4">
          <AttacksCard />
        </div>
      </div>
    </div>
  );
}
