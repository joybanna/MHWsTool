import { Sidebar } from "./ui/Sidebar";

export function Header() {
  return (
    <header className="sticky top-0 z-1 sm:hidden">
      {/* <div className="bg-neutral-400 p-1 text-center text-sm text-black">
        {text.HEADER}
        </div> */}
      <div className="bg-content flex items-center justify-between gap-2 p-3">
        <div className="flex items-center gap-2">
          <Sidebar />
        </div>
        {/* <Button asChild variant="text" className="p-0">
          <a
            href="https://github.com/chanleyou/mhwilds-calculator"
            target="_blank"
            rel="noopener noreferrer"
          >
            <Image
              src="/github.svg"
              alt="GitHub"
              height={24}
              width={24}
              className="invert"
            />
          </a>
        </Button> */}
      </div>
    </header>
  );
}
