"use client";

import { CircleCheckIcon, InfoIcon, TriangleAlertIcon } from "lucide-react";
import React from "react";
import { ExternalToast, toast as sonnerToast } from "sonner";

type ToastProps = {
  id: string | number;
  title: React.ReactNode;
  description?: React.ReactNode;
  type: "success" | "error" | "warning" | "info";
};

const Toast = ({ title, description, type }: ToastProps) => {
  return (
    <div className="bg-content-alt border-divider text-primary flex max-w-full min-w-90 flex-col gap-1 border p-4 font-sans">
      <div className="flex items-center gap-2">
        {type === "info" && <InfoIcon className="stroke-info size-5" />}
        {type === "success" && (
          <CircleCheckIcon className="stroke-success size-5" />
        )}
        {type === "error" && (
          <TriangleAlertIcon className="stroke-error size-5" />
        )}
        {type === "warning" && (
          <TriangleAlertIcon className="stroke-warning size-5 shrink-0" />
        )}

        <div className="flex flex-col gap-0.5">
          {typeof title === "string" ? <h2>{title}</h2> : title}
          {typeof description === "string" ? (
            <p className="text-tertiary text-sm">{description}</p>
          ) : (
            description
          )}
        </div>
      </div>
    </div>
  );
};

export const toast = (toast: Omit<ToastProps, "id">, data?: ExternalToast) => {
  return sonnerToast.custom((id) => <Toast id={id} {...toast} />, data);
};
