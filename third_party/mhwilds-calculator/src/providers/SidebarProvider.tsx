"use client";

import { createContext, useContext, useState } from "react";
import useIsMobile from "@/components/hooks/useIsMobile";

export const SidebarContext = createContext<{
  open: boolean;
  setOpen: (open: boolean) => void;
  compact: boolean;
  setCompact: (compact: boolean) => void;
}>({
  open: false,
  setOpen: () => {},
  compact: false,
  setCompact: () => {},
});

export const SidebarProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const isMobile = useIsMobile();
  const [isMobileOpen, setIsMobileOpen] = useState(false);
  const [compact, setCompact] = useState(true);

  const open = isMobileOpen && isMobile;

  const isExpandedCompact = compact && !isMobile;

  return (
    <SidebarContext.Provider
      value={{
        open,
        setOpen: setIsMobileOpen,
        compact: isExpandedCompact,
        setCompact,
      }}
    >
      {children}
    </SidebarContext.Provider>
  );
};

export const useSidebar = () => useContext(SidebarContext);
