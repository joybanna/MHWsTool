"use client";

// import { useState } from "react";
import {
  AttacksCard,
  BuffsCard,
  ManualSkillsCard,
  ManualWeaponCard,
  // Notice,
  SkillPointCard,
  StatsCard,
  // Tab,
} from "@/components";

export default function Calculator() {
  return (
    <div className="max-w-9xl mx-auto flex w-full flex-col gap-2 p-4">
      <div className="flex flex-col gap-2 xl:flex-row">
        <div className="flex flex-4 flex-col gap-2">
          <ManualWeaponCard />
          <ManualSkillsCard />
          <BuffsCard />
        </div>
        <div className="flex flex-2 flex-col gap-2">
          <StatsCard />
          <SkillPointCard />
          {/* <Card>
            <h1>Debug</h1>
            <h1>Total Weight: {totalWeight}</h1>
            <textarea
              className="font-mono text-xs"
              value={JSON.stringify(weights, undefined, 2)}
              rows={60}
              readOnly
            />
            <textarea
              className="font-mono text-xs"
              value={JSON.stringify(head, undefined, 2)}
              rows={60}
              readOnly
            />
          </Card> */}
        </div>
        <div className="flex-3">
          <AttacksCard />
        </div>
      </div>
    </div>
  );
}
