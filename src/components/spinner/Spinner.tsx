import clsx from "clsx";
import type React from "react";

type SpinnerProps = {
  className?: string;
};

export const Spinner: React.FC<SpinnerProps> = ({ className }) => {
  return (
    <div
      className={clsx(
        "inline-block h-5 w-5 animate-spin rounded-full border-4 border-solid border-current border-r-transparent align-[-0.125em] text-obsidian-600 motion-reduce:animate-[spin_1.5s_linear_infinite]",
        className,
      )}
      role="status"
    >
      <span className="!absolute !-m-px !h-px !w-px !overflow-hidden !whitespace-nowrap !border-0 !p-0 ![clip:rect(0,0,0,0)]">
        Loading...
      </span>
    </div>
  );
};
