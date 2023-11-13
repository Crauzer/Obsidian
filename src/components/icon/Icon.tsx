import clsx from 'clsx';
import { match } from 'ts-pattern';

import { Size } from '../../types';

export type IconProps = {
  size: Size;

  icon: React.ComponentType<
    React.SVGProps<SVGSVGElement> & {
      title?: string | undefined;
    }
  >;

  className?: string;
} & React.SVGProps<SVGSVGElement>;

export const Icon: React.FC<IconProps> = ({
  size = 'md',
  icon: IconComponent,
  className,
  ...props
}) => {
  return <IconComponent {...props} className={clsx(className, getWidthFromSize(size))} />;
};

const getWidthFromSize = (size: Size) =>
  match(size)
    .with('xs', () => clsx('w-[12px] h-[12px]'))
    .with('sm', () => clsx('w-[14px] h-[14px]'))
    .with('md', () => clsx('w-[16px] h-[16px]'))
    .with('lg', () => clsx('w-[20px] h-[20px]'))
    .with('xl', () => clsx('w-[24px] h-[24px]'))
    .exhaustive();
