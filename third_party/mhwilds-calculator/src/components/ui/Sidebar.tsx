"use client";

import { ChevronsLeftIcon, MenuIcon } from "lucide-react";
import { useSidebar } from "@/providers/SidebarProvider";
import { cn } from "@/utils";
import { SidebarContent } from "../SidebarContent";
import { Button } from "./Button";
import { Sheet, SheetContent, SheetTitle, SheetTrigger } from "./Sheet";

export function Sidebar() {
  const { open, setOpen } = useSidebar();

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button
          size="icon"
          variant="text"
          className="text-primary"
          onClick={() => setOpen(true)}
        >
          <MenuIcon />
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="py-2">
        <SheetTitle
          className={cn(
            "mb-0.5 flex items-center justify-between pl-2 text-base!",
          )}
        >
          MHW: Damage Calculator
          <Button variant="text" onClick={() => setOpen(!open)}>
            <ChevronsLeftIcon className="size-5" />
          </Button>
        </SheetTitle>
        <SidebarContent />
      </SheetContent>
    </Sheet>
  );
}
