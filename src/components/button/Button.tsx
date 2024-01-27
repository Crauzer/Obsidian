import clsx from 'clsx';
import React, { forwardRef } from 'react';

import {
  ClickableVariant,
  ElementRef,
  JustifyContent,
  PolymorphicPropsWithRef,
  Size,
} from '../../types';
import { getClickableVariantClass, getJustifyClass, getTextSizeClass } from '../../utils';

type BaseButtonProps = {
  variant?: ClickableVariant;
  size?: Size;
  justify?: JustifyContent;

  fullWidth?: boolean;
  disabled?: boolean;
  compact?: boolean;

  left?: React.ReactNode;
  right?: React.ReactNode;
};

export type ButtonProps<TAs extends React.ElementType> = PolymorphicPropsWithRef<
  TAs,
  BaseButtonProps
> &
  React.DetailedHTMLProps<React.ButtonHTMLAttributes<HTMLButtonElement>, HTMLButtonElement>;

type ButtonComponent = <TAs extends React.ElementType = 'button'>(
  props: ButtonProps<TAs>,
) => React.ReactNode;

export const Button: ButtonComponent = forwardRef(
  <TAs extends React.ElementType = 'button'>(
    {
      className,
      as,
      variant = 'default',
      size = 'md',
      justify = 'center',
      fullWidth = false,
      compact = false,
      left,
      right,
      children,
      ...buttonProps
    }: ButtonProps<TAs>,
    ref: ElementRef<TAs>,
  ) => {
    const Component = as || 'button';

    return (
      <Component
        {...buttonProps}
        ref={ref}
        className={clsx(
          'flex items-center rounded-md border transition-colors duration-150',
          getTextSizeClass(size),
          getJustifyClass(justify),
          {
            'p-2': compact,
            'px-4 py-2': !compact,
          },
          {
            'w-full': fullWidth,
          },
          getClickableVariantClass(variant),
          className,
        )}
      >
        <span className="flex flex-row items-center justify-center gap-2">
          {left}
          {children}
          {right}
        </span>
      </Component>
    );
  },
);
