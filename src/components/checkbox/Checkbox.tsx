import * as RadixCheckbox from '@radix-ui/react-checkbox';
import * as RadixLabel from '@radix-ui/react-label';
import clsx from 'clsx';
import { forwardRef, useId, useState } from 'react';
import { FaCheck } from 'react-icons/fa';

import { Icon } from '..';

export type CheckboxProps = RadixCheckbox.CheckboxProps;

export const Checkbox = forwardRef<HTMLButtonElement, CheckboxProps>(
  ({ checked: checkedOriginal, onCheckedChange, className, children, ...props }, ref) => {
    const id = useId();

    const [checked, setChecked] = useState(checkedOriginal);

    const handleCheckedChange = (checked: RadixCheckbox.CheckedState) => {
      setChecked(checked);
      onCheckedChange?.(checked);
    };

    return (
      <div className={clsx(className, 'flex flex-row items-center gap-2')}>
        <RadixCheckbox.Root
          {...props}
          id={id}
          ref={ref}
          className={clsx(
            'flex h-6 w-6 items-center justify-center transition-colors',
            'rounded border border-gray-600 bg-gray-700',
            !checked && 'hover:border-obsidian-500 hover:bg-gray-600',
            checked && 'border-obsidian-500 bg-obsidian-500',
          )}
          checked={checked}
          onCheckedChange={handleCheckedChange}
        >
          <RadixCheckbox.Indicator className="block min-h-4 min-w-4 rounded text-gray-800">
            <Icon size="sm" icon={FaCheck} />
          </RadixCheckbox.Indicator>
        </RadixCheckbox.Root>
        <RadixLabel.Root className="cursor-pointer text-sm text-gray-50" htmlFor={id}>
          {children}
        </RadixLabel.Root>
      </div>
    );
  },
);
