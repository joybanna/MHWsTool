import { TooltipProvider } from "@radix-ui/react-tooltip";
import type { Metadata } from "next";
import { Toaster } from "sonner";
import { SidebarProvider } from "@/providers/SidebarProvider";
import "./globals.css";

export const metadata: Metadata = {
  title: "MH: Wilds Damage Calculator & Build Planner",
  description:
    "A damage calculator and build planner for Monster Hunter: Wilds.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <TooltipProvider delayDuration={100}>
        <SidebarProvider>
          <body
            className="text-primary bg-background flex antialiased"
          >
            <main className="flex h-screen min-h-screen flex-1 flex-col overflow-y-auto">
              {children}
            </main>
            {/* <Footer /> */}
            <Toaster
              position="bottom-right"
              expand={true}
              visibleToasts={3}
              offset="4rem"
            />
          </body>
        </SidebarProvider>
      </TooltipProvider>
    </html>
  );
}
