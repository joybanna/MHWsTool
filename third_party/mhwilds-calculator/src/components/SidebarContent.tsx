"use client";

import { CalculatorIcon, WrenchIcon } from "lucide-react";
import Image from "next/image";
import { usePathname } from "next/navigation";
import { ExportDialog } from "./ExportDialog";
import { ImportDialog } from "./ImportDialog";
import { OverridesDialog } from "./OverridesDialog";
import { Separator } from "./ui/Separator";
import { SidebarButton } from "./ui/SidebarButton";

export const SidebarContent = () => {
  const pathname = usePathname();

  return (
    <>
      <Separator className="my-2" />
      <div className="flex h-full flex-col justify-between gap-2">
        <div className="flex flex-col gap-2">
          <div className="flex flex-col">
            <div className="flex flex-col px-2">
              <SidebarButton
                href="/"
                icon={WrenchIcon}
                text="Builder"
                internal
              />
              <SidebarButton
                href="/calc"
                icon={CalculatorIcon}
                text="Calculator"
                internal
              />
            </div>
            <Separator className="my-2" />
            {pathname === "/" && (
              <div className="flex flex-col px-2">
                <OverridesDialog />
                <ExportDialog />
                <ImportDialog />
              </div>
            )}
          </div>
        </div>
        <div className="flex flex-col px-2">
          <SidebarButton
            href="https://github.com/chanleyou/mhwilds-calculator"
            icon={() => (
              <Image
                src="/github.svg"
                alt="GitHub"
                height={20}
                width={20}
                className="invert"
              />
            )}
            text="GitHub"
          />
        </div>
      </div>
    </>
  );
};
