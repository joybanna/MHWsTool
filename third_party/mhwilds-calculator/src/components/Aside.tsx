"use client";

import { ChevronsLeftIcon } from "lucide-react";
import { useSidebar } from "@/providers/SidebarProvider";
import { cn } from "@/utils";
import { SidebarContent } from "./SidebarContent";
import { Button } from "./ui/Button";

export default function Aside() {
  const { compact, setCompact } = useSidebar();

  return (
    <aside
      data-state={compact ? "closed" : "open"}
      className={cn(
        "bg-content border-content-alt hidden flex-col overflow-x-hidden overflow-y-auto border-r py-2 sm:flex",
        "ease transition-[width] duration-300 ease-in-out",
        "data-[state=closed]:w-[53px] data-[state=open]:w-66",
      )}
    >
      <div
        className={cn(
          "mb-0.5 flex items-center justify-between",
          compact ? "justify-center" : "pl-2",
        )}
      >
        {!compact && <h1 className="truncate">MHW: Damage Calculator</h1>}
        <Button
          className={cn("transition", compact && "rotate-180 px-2")}
          variant="text"
          onClick={() => setCompact(!compact)}
        >
          <ChevronsLeftIcon className="size-5" />
        </Button>
      </div>
      <SidebarContent />
    </aside>
  );
}
