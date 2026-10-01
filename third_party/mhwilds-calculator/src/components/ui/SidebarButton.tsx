"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSidebar } from "@/providers/SidebarProvider";
import { cn } from "@/utils";
import {
  Tooltip,
  TooltipArrow,
  TooltipContent,
  TooltipTrigger,
} from "../Tooltip";
import { Button } from "./Button";

type Props = React.ComponentProps<typeof Button> & {
  icon: React.FC<React.SVGProps<SVGSVGElement>>;
  text: string;
  href?: string;
  internal?: boolean;
};

export const SidebarButton = ({
  icon,
  text,
  href,
  internal,
  ...props
}: Props) => {
  const pathname = usePathname();
  const { compact } = useSidebar();

  const Icon = icon;

  return (
    <Tooltip disableHoverableContent={!compact}>
      <TooltipTrigger asChild>
        <Button
          variant="text"
          {...props}
          asChild={!!href}
          className={cn(
            "hover:bg-divider gap-2 rounded px-2 hover:opacity-100",
            "justify-start",
            pathname === href && "bg-content-alt",
          )}
        >
          {href ? (
            <Link href={href} target={internal ? undefined : "_blank"}>
              <Icon className="size-5" />
              {!compact && <label className="truncate">{text}</label>}
            </Link>
          ) : (
            <>
              <Icon className="size-5" />
              {!compact && <label className="truncate">{text}</label>}
            </>
          )}
        </Button>
      </TooltipTrigger>
      {compact && (
        <TooltipContent sideOffset={12} side="right">
          <TooltipArrow />
          {text}
        </TooltipContent>
      )}
    </Tooltip>
  );
};
