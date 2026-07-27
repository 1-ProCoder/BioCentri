export default function Container({ children, className = '', as: Tag = 'div' }) {
  return (
    <Tag className={'mx-auto max-w-6xl px-6 md:px-8 ' + className}>
      {children}
    </Tag>
  );
}
