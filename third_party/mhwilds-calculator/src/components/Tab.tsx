import { cn } from "@/utils";
import { Button } from ".";

export function Tab<T>({
  value,
  options,
  labelFn = (o) => String(o),
  setValue,
}: {
  options: [T, T];
  value: T;
  labelFn?: (option: T) => string;
  setValue: (option: T) => void;
}) {
  return (
    <div className="border-divider flex border p-0.5">
      {options.map((o) => (
        <Button
          // variant="text"
          size="sm"
          onClick={() => setValue(o)}
          variant="secondary"
          key={labelFn(o)}
          className={cn(
            "flex-1 rounded-none border-none",
            value === o ? "bg-content-alt text-primary" : "text-tertiary",
          )}
        >
          {labelFn(o)}
        </Button>
      ))}
    </div>
  );
}
