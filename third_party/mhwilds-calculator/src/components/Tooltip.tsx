"use client";

import * as TooltipPrimitive from "@radix-ui/react-tooltip";
import { cn } from "@/utils";

interface TooltipContentProps extends TooltipPrimitive.TooltipContentProps {
  ref?: React.RefObject<HTMLDivElement>;
}

export const Tooltip = TooltipPrimitive.Root;

export const TooltipTrigger = TooltipPrimitive.Trigger;

export const TooltipArrow = ({
  className,
  ...props
}: TooltipPrimitive.TooltipArrowProps) => {
  return (
    <TooltipPrimitive.Arrow
      className={cn("h-1.5 w-2 fill-zinc-100", className)}
      {...props}
    />
  );
};

export const TooltipContent = ({
  className,
  ...props
}: TooltipContentProps) => {
  return (
    <TooltipPrimitive.Content
      className={cn(
        "animate-in fade-in-0 zoom-in-95 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 text-background data-[state=closed]:zoom-out-95 z-50 max-w-[90vw] bg-zinc-100 px-2 py-1.5 text-sm drop-shadow duration-100",
        className,
      )}
      {...props}
    />
  );
};

TooltipContent.displayName = TooltipPrimitive.Content.displayName;
