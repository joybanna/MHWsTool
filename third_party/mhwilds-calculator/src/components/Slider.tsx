"use client";

import * as SliderPrimitive from "@radix-ui/react-slider";
import { useCallback, useEffect, useRef, useState } from "react";
import { SkillName } from "@/types";
import { cn } from "@/utils";

type SliderProps = SliderPrimitive.SliderProps & {
  skill?: SkillName;
  ref?: React.Ref<HTMLDivElement>;
  tooltip?: string;
  debounceMs?: number;
};

export const Slider = ({
  className,
  ref,
  value,
  tooltip,
  onValueChange,
  debounceMs = 200,
  ...props
}: SliderProps) => {
  const [debouncedValue, setDebouncedValue] = useState(value);

  const timeoutRef = useRef<NodeJS.Timeout | undefined>(undefined);

  const debouncedOnValueChange = useCallback(
    (newValue: number[]) => {
      setDebouncedValue(newValue);

      if (timeoutRef.current) clearTimeout(timeoutRef.current);

      timeoutRef.current = setTimeout(
        () => onValueChange?.(newValue),
        debounceMs,
      );
    },
    [onValueChange, debounceMs],
  );

  useEffect(() => {
    return () => clearTimeout(timeoutRef.current);
  }, []);

  return (
    <div className="flex w-full items-center gap-2">
      <SliderPrimitive.Root
        ref={ref}
        className={cn(
          "relative flex w-full touch-none items-center select-none",
          className,
        )}
        value={debouncedValue}
        onValueChange={debouncedOnValueChange}
        {...props}
      >
        <SliderPrimitive.Track className="bg-divider relative h-1 w-full grow overflow-hidden rounded-full">
          <SliderPrimitive.Range className="bg-secondary absolute h-full" />
        </SliderPrimitive.Track>
        <SliderPrimitive.Thumb className="bg-primary ring-offset-background focus:bg-accent block size-4 rounded-full focus-visible:outline-none disabled:pointer-events-none disabled:opacity-50" />
      </SliderPrimitive.Root>
      <div className="text-sm">{debouncedValue}%</div>
    </div>
  );
};
