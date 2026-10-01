import { XIcon } from "lucide-react";
import Image from "next/image";
import { useCallback, useEffect, useMemo, useState } from "react";
import { Armors } from "@/data/armor";
import { Armor, ArmorType } from "@/types";
import { cn } from "@/utils";
import { Picker } from "./Picker";
import { TextInput } from "./TextInput";
import { Button } from "./ui/Button";
import { Dialog, DialogContent, DialogTrigger } from "./ui/Dialog";

export const ArmorPickerDialog = ({
  type,
  value,
  setValue,
}: {
  type: ArmorType;
  value?: Armor;
  setValue: (value?: Armor) => void;
}) => {
  const [open, setOpen] = useState(false);
  const [filter, setFilter] = useState("");

  const filteredOptions = useMemo(() => {
    return Armors.filter((a) => {
      if (a.type !== type) return false;
      if (filter) {
        const { name, groupSkill, seriesSkills, skills } = a;
        const search = [
          name,
          groupSkill,
          ...(seriesSkills ?? []),
          ...Object.entries(skills).map(([k, v]) => `${k} ${v}`),
        ]
          .filter((k) => !!k)
          .join(" ")
          .toLowerCase();

        return search.includes(filter.toLowerCase());
      }

      return true;
    });
  }, [filter, type]);

  useEffect(() => void setFilter(""), [open]);

  const clear = useCallback(
    (e: React.MouseEvent) => {
      e.stopPropagation();
      setValue(undefined);
    },
    [setValue],
  );

  const rowCn = cn(
    "border-content-alt flex flex-row justify-between gap-3 border-b p-2 last:border-0",
  );

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Picker className={cn(!value && "text-placeholder")}>
          <div className="flex items-center gap-1.5">
            <Image
              src={`/${type.toLowerCase()}.svg`}
              alt={type}
              width={20}
              height={20}
            />
            {value ? value.name : type}
          </div>
          {value && (
            <Button
              asChild
              variant="text"
              size="icon"
              onClick={clear}
              className="text-secondary font-bold"
            >
              <div>
                <XIcon size={16} />
              </div>
            </Button>
          )}
          {/* {value?.skills &&
            Object.entries(value.skills).map(([k, v]) => (
              <p className="text-secondary text-xs" key={k}>{`${k} ${v}`}</p>
            ))} */}
        </Picker>
      </DialogTrigger>
      <DialogContent setOpen={setOpen} title={`Select ${type}`}>
        <TextInput
          value={filter}
          onChangeValue={setFilter}
          placeholder={"Search..."}
          autoFocus
        />
        <div className="grid gap-1 overflow-y-auto pr-2 text-sm md:grid-cols-2">
          {filteredOptions.map((o) => (
            <div
              className="border-divider hover:border-primary cursor-pointer gap-1 border p-3"
              key={o.name}
              onClick={() => {
                setValue(o);
                setOpen(false);
              }}
            >
              <div className={rowCn}>
                <div className="text-tertiary flex-1">Name</div>
                <div className="flex-3">{o.name}</div>
              </div>
              <div className={rowCn}>
                <div className="text-tertiary flex-1">Skills</div>
                <div className="flex-3">
                  {Object.entries(o.skills).map(([k, v]) => (
                    <p className="text-sm" key={k + v}>
                      {k} {v}
                    </p>
                  ))}
                </div>
              </div>
              {o.seriesSkills && (
                <div className={rowCn}>
                  <div className="text-tertiary flex-1">Series</div>
                  <div className="flex-3">
                    {o.seriesSkills?.map((ss) => (
                      <p key={ss} className="text-sm">
                        {ss}
                      </p>
                    ))}
                  </div>
                </div>
              )}
              {o.groupSkill && (
                <div className={rowCn}>
                  <div className="text-tertiary flex-1">Group</div>
                  <div className="flex-3">{o.groupSkill}</div>
                </div>
              )}
              <div className={rowCn}>
                <div className="text-tertiary flex-1">Slots</div>
                <div className="flex-3">
                  {o.slots.some((n) => n > 0)
                    ? o.slots.filter((s) => s > 0).join("-")
                    : "-"}
                </div>
              </div>
            </div>
          ))}
        </div>
      </DialogContent>
    </Dialog>
  );
};
