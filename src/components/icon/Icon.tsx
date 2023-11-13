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
  return (
    <IconComponent
      {...props}
      className={className}
      width={getWidthFromSize(size)}
      height={getWidthFromSize(size)}
    />
  );
};

const getWidthFromSize = (size: Size) =>
  match(size)
    .with('xs', () => 12)
    .with('sm', () => 14)
    .with('md', () => 16)
    .with('lg', () => 20)
    .with('xl', () => 24)
    .exhaustive();
