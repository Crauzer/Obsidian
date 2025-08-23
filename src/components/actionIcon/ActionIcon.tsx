import clsx from "clsx";
import type React from "react";
import { forwardRef } from "react";

import { Button, type ButtonProps, Icon } from "..";

export type ActionIconProps = {
  icon: React.ComponentType<
    React.SVGProps<SVGSVGElement> & {
      title?: string | undefined;
    }
  >;
  iconClassName?: string;
} & ButtonProps<"button">;

export const ActionIcon = forwardRef<HTMLButtonElement, ActionIconProps>(
  ({ size = "md", variant, icon, iconClassName, ...props }, ref) => {
    return (
      <Button
        {...props}
        ref={ref}
        compact
        className={clsx(props.className, "fill-gray-200")}
        size={size}
        variant={variant}
      >
        <Icon size={size} icon={icon} className={iconClassName} />
      </Button>
    );
  },
);
