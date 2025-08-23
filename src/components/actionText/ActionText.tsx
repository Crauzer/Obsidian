import clsx from "clsx";
import type React from "react";

export type ActionTextProps = {
  onClick?: () => void;

  className?: string;

  children?: React.ReactNode;
};

export const ActionText: React.FC<ActionTextProps> = ({
  onClick,
  className,
  children,
}) => {
  return (
    <span
      className={clsx(
        "font-bold text-obsidian-500 transition-colors hover:text-obsidian-600 hover:underline",
        className,
      )}
      onClick={onClick}
    >
      {children}
    </span>
  );
};
