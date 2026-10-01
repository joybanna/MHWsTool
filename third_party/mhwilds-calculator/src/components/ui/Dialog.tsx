"use client";

import * as DialogPrimitive from "@radix-ui/react-dialog";
import { XIcon } from "lucide-react";
import React from "react";
import { cn } from "@/utils";
import { Button } from "./Button";
import { Card } from "./Card";

export const Dialog = DialogPrimitive.Root;

export const DialogTrigger = DialogPrimitive.Trigger;

export const DialogTitle = DialogPrimitive.Title;

type DialogContentProps = DialogPrimitive.DialogContentProps & {
  title?: React.ReactNode;
  setOpen?: (n: boolean) => void;
};

export const DialogContent = ({
  className,
  title,
  setOpen,
  children,
  ...props
}: DialogContentProps) => (
  <DialogPrimitive.Portal>
    <DialogPrimitive.Overlay className="data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 fixed inset-0 z-50 flex items-center justify-center bg-black/75">
      <DialogPrimitive.Content
        className="data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95"
        {...props}
      >
        <Card
          className={cn(
            "h-dvh w-[100vw] rounded-none sm:h-[85dvh] sm:w-4xl sm:max-w-[95vw]",
            className,
          )}
        >
          {(title || setOpen) && (
            <div className="mb-2 flex items-start justify-between gap-2">
              <DialogTitle asChild>
                <h1>{title}</h1>
              </DialogTitle>
              {setOpen && (
                <Button
                  variant="text"
                  size="icon"
                  onClick={() => setOpen(false)}
                >
                  <XIcon className="size-5" />
                </Button>
              )}
            </div>
          )}
          {children}
        </Card>
      </DialogPrimitive.Content>
    </DialogPrimitive.Overlay>
  </DialogPrimitive.Portal>
);
